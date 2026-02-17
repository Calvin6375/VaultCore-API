using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using VaultCore.Application.Common;
using VaultCore.Application.DTOs;
using VaultCore.Application.Mapping;
using VaultCore.Application.Services;
using VaultCore.Domain.Entities;
using VaultCore.Domain.Enums;
using VaultCore.Domain.Interfaces;
using Xunit;

namespace VaultCore.UnitTests.Services;

public class TransactionServiceTests
{
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<ICurrentUserService> _currentUser;
    private readonly Mock<IAuditService> _auditService;
    private readonly IMapper _mapper;

    public TransactionServiceTests()
    {
        _uow = new Mock<IUnitOfWork>();
        _currentUser = new Mock<ICurrentUserService>();
        _auditService = new Mock<IAuditService>();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task WithdrawAsync_WhenInsufficientBalance_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var wallet = new Wallet { Id = Guid.NewGuid(), UserId = userId, Balance = 10, Status = WalletStatus.Active };
        _currentUser.Setup(x => x.UserId).Returns(userId);
        _uow.Setup(x => x.Wallets.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);
        var walletRepo = new Mock<IWalletRepository>();
        var txRepo = new Mock<ITransactionRepository>();
        _uow.Setup(x => x.Wallets).Returns(walletRepo.Object);
        _uow.Setup(x => x.Transactions).Returns(txRepo.Object);

        var sut = new TransactionService(_uow.Object, _currentUser.Object, _auditService.Object, _mapper, Mock.Of<ILogger<TransactionService>>());
        var request = new WithdrawalRequest(100);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.WithdrawAsync(request));
    }

    [Fact]
    public async Task WithdrawAsync_WhenSufficientBalance_DecrementsBalanceAndReturnsTransaction()
    {
        var userId = Guid.NewGuid();
        var wallet = new Wallet { Id = Guid.NewGuid(), UserId = userId, Balance = 100, Status = WalletStatus.Active, CurrencyCode = "KES" };
        _currentUser.Setup(x => x.UserId).Returns(userId);
        _uow.Setup(x => x.Wallets.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);
        _uow.Setup(x => x.Transactions.GetByIdempotencyKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((Transaction?)null);
        _uow.Setup(x => x.Transactions.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>())).ReturnsAsync((Transaction t, CancellationToken _) => t);
        _uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var walletRepo = new Mock<IWalletRepository>();
        var txRepo = new Mock<ITransactionRepository>();
        _uow.Setup(x => x.Wallets).Returns(walletRepo.Object);
        _uow.Setup(x => x.Transactions).Returns(txRepo.Object);

        var sut = new TransactionService(_uow.Object, _currentUser.Object, _auditService.Object, _mapper, Mock.Of<ILogger<TransactionService>>());
        var request = new WithdrawalRequest(30);

        var result = await sut.WithdrawAsync(request);

        Assert.NotNull(result);
        Assert.Equal(-30, result.Amount);
        Assert.Equal(TransactionStatus.Completed, result.Status);
        Assert.Equal(70, result.BalanceAfter);
    }
}
