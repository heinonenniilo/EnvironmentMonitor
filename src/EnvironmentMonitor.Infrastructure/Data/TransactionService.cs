using EnvironmentMonitor.Domain.Interfaces;

namespace EnvironmentMonitor.Infrastructure.Data
{
    public class TransactionService : ITransactionService
    {
        private readonly MeasurementDbContext _context;

        public TransactionService(MeasurementDbContext context)
        {
            _context = context;
        }

        public async Task<IUnitOfWorkTransaction> BeginTransactionAsync()
        {
            var transaction = await _context.Database.BeginTransactionAsync();
            return new EfTransaction(transaction);
        }
    }
}
