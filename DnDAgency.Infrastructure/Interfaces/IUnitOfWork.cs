// IUnitOfWork.cs
using DnDAgency.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace DnDAgency.Infrastructure.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ICampaignRepository Campaigns { get; }
        ISlotRepository Slots { get; }
        IMasterRepository Masters { get; }
        // Добавьте другие репозитории по необходимости

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}