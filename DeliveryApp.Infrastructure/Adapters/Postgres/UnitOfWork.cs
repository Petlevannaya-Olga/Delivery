using Primitives;

namespace DeliveryApp.Infrastructure.Adapters.Postgres
{
    public class UnitOfWork(ApplicationDbContext dbContext) : IUnitOfWork, IDisposable
    {
        private readonly ApplicationDbContext _dbContext =
            dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _dbContext.Database.CommitTransactionAsync(cancellationToken);
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            await _dbContext.Database.CommitTransactionAsync(cancellationToken);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing) _dbContext.Dispose();
                _disposed = true;
            }
        }
    }
}