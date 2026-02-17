using AutoMapper;
using Microsoft.Extensions.Logging;
using VaultCore.Application.Common;
using VaultCore.Application.DTOs;
using VaultCore.Application.DTOs.Auth;
using VaultCore.Domain.Entities;
using VaultCore.Domain.Enums;
using VaultCore.Domain.Interfaces;

namespace VaultCore.Application.Services;

/// <summary>
/// Handles registration, login, refresh token, and logout.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUnitOfWork uow,
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        IAuditService auditService,
        IMapper mapper,
        ILogger<AuthService> logger)
    {
        _uow = uow;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _auditService = auditService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _uow.Users.GetByEmailAsync(request.Email, cancellationToken: cancellationToken);
        if (existing != null)
            throw new InvalidOperationException("User with this email already exists.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.Normalize().ToLowerInvariant(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            KycStatus = KycStatus.Pending,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        await _uow.Users.AddAsync(user, cancellationToken);

        var customerRole = await _uow.Roles.GetByNameAsync(RoleType.Customer, cancellationToken);
        if (customerRole == null)
            throw new InvalidOperationException("Customer role not found. Ensure database is seeded.");

        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = customerRole.Id,
            AssignedAtUtc = DateTime.UtcNow
        };
        user.UserRoles.Add(userRole);

        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CurrencyCode = "KES",
            Balance = 0,
            Status = WalletStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
        await _uow.Wallets.AddAsync(wallet, cancellationToken);

        await _uow.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("User.Registered", "User", user.Id.ToString(), afterState: new { user.Email }, cancellationToken);
        _logger.LogInformation("User registered: {Email}", user.Email);

        var roles = new[] { "Customer" };
        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshTokenEntity = await _tokenService.CreateRefreshTokenAsync(user.Id, "Registration", cancellationToken);
        await _uow.RefreshTokens.AddAsync(refreshTokenEntity, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        var userDto = _mapper.Map<UserDto>(user);
        userDto = userDto with { Roles = roles };
        return new AuthResponse(
            accessToken,
            refreshTokenEntity.Token,
            refreshTokenEntity.ExpiresAtUtc,
            userDto
        );
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var user = await _uow.Users.GetByEmailAsync(request.Email, includeRoles: true, cancellationToken: cancellationToken);
        if (user == null || !user.IsActive || user.IsDeleted)
            return null;

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            return null;

        var roles = user.UserRoles.Select(ur => ur.Role.Name.ToString()).ToList();
        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshTokenEntity = await _tokenService.CreateRefreshTokenAsync(user.Id, ipAddress ?? "Unknown", cancellationToken);
        await _uow.RefreshTokens.AddAsync(refreshTokenEntity, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("User.Login", "User", user.Id.ToString(), afterState: new { Ip = ipAddress }, cancellationToken);
        var userDto = _mapper.Map<UserDto>(user);
        return new AuthResponse(accessToken, refreshTokenEntity.Token, refreshTokenEntity.ExpiresAtUtc, userDto);
    }

    public async Task<AuthResponse?> RefreshTokenAsync(string refreshToken, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var tokenEntity = await _uow.RefreshTokens.GetByTokenAsync(refreshToken, cancellationToken);
        if (tokenEntity == null || tokenEntity.IsRevoked || tokenEntity.ExpiresAtUtc < DateTime.UtcNow)
            return null;

        var user = await _uow.Users.GetByIdAsync(tokenEntity.UserId, includeRoles: true, cancellationToken: cancellationToken);
        if (user == null || !user.IsActive || user.IsDeleted)
            return null;

        tokenEntity.RevokedAtUtc = DateTime.UtcNow;
        tokenEntity.RevokedByIp = ipAddress;
        _uow.RefreshTokens.Update(tokenEntity);

        var roles = user.UserRoles.Select(ur => ur.Role.Name.ToString()).ToList();
        var newAccessToken = _tokenService.GenerateAccessToken(user, roles);
        var newRefreshToken = await _tokenService.CreateRefreshTokenAsync(user.Id, ipAddress ?? "Unknown", cancellationToken);
        await _uow.RefreshTokens.AddAsync(newRefreshToken, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        var userDto = _mapper.Map<UserDto>(user);
        return new AuthResponse(newAccessToken, newRefreshToken.Token, newRefreshToken.ExpiresAtUtc, userDto);
    }

    public async Task RevokeTokenAsync(string refreshToken, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var tokenEntity = await _uow.RefreshTokens.GetByTokenAsync(refreshToken, cancellationToken);
        if (tokenEntity == null)
            return;

        if (!tokenEntity.IsRevoked)
        {
            tokenEntity.RevokedAtUtc = DateTime.UtcNow;
            tokenEntity.RevokedByIp = ipAddress;
            _uow.RefreshTokens.Update(tokenEntity);
            await _uow.SaveChangesAsync(cancellationToken);
            await _auditService.LogAsync("User.Logout", "RefreshToken", tokenEntity.Id.ToString(), afterState: new { tokenEntity.UserId }, cancellationToken);
        }
    }
}
