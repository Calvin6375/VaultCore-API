using AutoMapper;
using VaultCore.Application.DTOs;
using VaultCore.Application.DTOs.Auth;
using VaultCore.Domain.Entities;
using VaultCore.Domain.Enums;

namespace VaultCore.Application.Mapping;

/// <summary>
/// AutoMapper profile for entity-DTO mappings.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(d => d.Roles, opt => opt.MapFrom(s => s.UserRoles.Select(ur => ur.Role.Name.ToString())));
        CreateMap<Wallet, WalletDto>();
        CreateMap<Transaction, TransactionDto>();
        CreateMap<AuditLog, AuditLogDto>();
    }
}
