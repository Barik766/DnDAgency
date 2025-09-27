using DnDAgency.Application.Interfaces;
using DnDAgency.Domain.Interfaces;
using DnDAgency.Infrastructure.Data;
using DnDAgency.Infrastructure.Interfaces;
using DnDAgency.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace DnDAgency.Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        private ICampaignRepository? _campaigns;
        private ISlotRepository? _slots;
        private IMasterRepository? _masters;
        private IUserRepository? _users;
        private IBookingRepository? _bookings;
        private IRefreshTokenRepository? _refreshTokens;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public ICampaignRepository Campaigns =>
            _campaigns ??= new CampaignRepository(_context);

        public ISlotRepository Slots =>
            _slots ??= new SlotRepository(_context);

        public IMasterRepository Masters =>
            _masters ??= new MasterRepository(_context);

        public IUserRepository Users =>
            _users ??= new UserRepository(_context);

        public IBookingRepository Bookings =>
            _bookings ??= new BookingRepository(_context);

        public IRefreshTokenRepository RefreshTokens =>
            _refreshTokens ??= new RefreshTokenRepository(_context);

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            return _transaction;
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}