using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using VaultCore.Application.Common;
using VaultCore.Application.DTOs.Auth;
using VaultCore.Application.Mapping;
using VaultCore.Application.Services;
using VaultCore.Domain.Entities;
using VaultCore.Domain.Enums;
using VaultCore.Domain.Interfaces;
using Xunit;

namespace VaultCore.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<ITokenService> _tokenService;
    private readonly Mock<IPasswordHasher> _passwordHasher;
    private readonly Mock<IAuditService> _auditService;
    private readonly IMapper _mapper;

    public AuthServiceTests()
    {
        _uow = new Mock<IUnitOfWork>();
        _tokenService = new Mock<ITokenService>();
        _passwordHasher = new Mock<IPasswordHasher>();
        _auditService = new Mock<IAuditService>();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailExists_ThrowsInvalidOperationException()
    {
        var request = new RegisterRequest("test@test.com", "Pass@123", "John", "Doe", null);
        _uow.Setup(x => x.Users.GetByEmailAsync(request.Email, false, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = Guid.NewGuid(), Email = request.Email });
        var userRepo = new Mock<IUserRepository>();
        var roleRepo = new Mock<IRoleRepository>();
        var walletRepo = new Mock<IWalletRepository>();
        var refreshRepo = new Mock<IRefreshTokenRepository>();
        _uow.Setup(x => x.Users).Returns(userRepo.Object);
        _uow.Setup(x => x.Roles).Returns(roleRepo.Object);
        _uow.Setup(x => x.Wallets).Returns(walletRepo.Object);
        _uow.Setup(x => x.RefreshTokens).Returns(refreshRepo.Object);

        var sut = new AuthService(_uow.Object, _tokenService.Object, _passwordHasher.Object, _auditService.Object, _mapper, Mock.Of<ILogger<AuthService>>());

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.RegisterAsync(request));
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailNew_CreatesUserAndReturnsAuthResponse()
    {
        var request = new RegisterRequest("new@test.com", "Pass@123", "Jane", "Doe", null);
        _uow.Setup(x => x.Users.GetByEmailAsync(request.Email, false, false, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        _passwordHasher.Setup(x => x.Hash(It.IsAny<string>())).Returns("hashed");
        var role = new Role { Id = Guid.NewGuid(), Name = RoleType.Customer };
        _uow.Setup(x => x.Roles.GetByNameAsync(RoleType.Customer, It.IsAny<CancellationToken>())).ReturnsAsync(role);
        var userRepo = new Mock<IUserRepository>();
        var roleRepo = new Mock<IRoleRepository>();
        var walletRepo = new Mock<IWalletRepository>();
        var refreshRepo = new Mock<IRefreshTokenRepository>();
        _uow.Setup(x => x.Users).Returns(userRepo.Object);
        _uow.Setup(x => x.Roles).Returns(roleRepo.Object);
        _uow.Setup(x => x.Wallets).Returns(walletRepo.Object);
        _uow.Setup(x => x.RefreshTokens).Returns(refreshRepo.Object);
        _uow.Setup(x => x.Users.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).ReturnsAsync((User u, CancellationToken _) => u);
        _uow.Setup(x => x.Wallets.AddAsync(It.IsAny<Wallet>(), It.IsAny<CancellationToken>())).ReturnsAsync((Wallet w, CancellationToken _) => w);
        var refreshToken = new RefreshToken { Token = "ref", ExpiresAtUtc = DateTime.UtcNow.AddDays(7) };
        _tokenService.Setup(x => x.CreateRefreshTokenAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(refreshToken);
        _tokenService.Setup(x => x.GenerateAccessToken(It.IsAny<User>(), It.IsAny<IReadOnlyList<string>>())).Returns("access");
        _uow.Setup(x => x.RefreshTokens.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>())).ReturnsAsync((RefreshToken r, CancellationToken _) => r);
        _uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var sut = new AuthService(_uow.Object, _tokenService.Object, _passwordHasher.Object, _auditService.Object, _mapper, Mock.Of<ILogger<AuthService>>());

        var result = await sut.RegisterAsync(request);

        Assert.NotNull(result);
        Assert.Equal("access", result.AccessToken);
        Assert.Equal("ref", result.RefreshToken);
        userRepo.Verify(x => x.AddAsync(It.Is<User>(u => u.Email == "new@test.com" && u.FirstName == "Jane"), It.IsAny<CancellationToken>()), Times.Once);
    }
}
