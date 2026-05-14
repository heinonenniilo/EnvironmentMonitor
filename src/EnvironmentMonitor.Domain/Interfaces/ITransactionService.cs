namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface ITransactionService
    {
        Task<IUnitOfWorkTransaction> BeginTransactionAsync();
    }
}
