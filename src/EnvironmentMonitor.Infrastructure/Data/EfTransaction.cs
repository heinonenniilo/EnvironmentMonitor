using EnvironmentMonitor.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace EnvironmentMonitor.Infrastructure.Data
{
    public class EfTransaction : IUnitOfWorkTransaction
    {
        private readonly IDbContextTransaction _transaction;

        public EfTransaction(IDbContextTransaction transaction)
        {
            _transaction = transaction;
        }

        public void Commit() => _transaction.Commit();

        public Task CommitAsync() => _transaction.CommitAsync();

        public void Dispose() => _transaction.Dispose();

        public ValueTask DisposeAsync() => _transaction.DisposeAsync();
    }
}
