namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface IUnitOfWorkTransaction : IDisposable, IAsyncDisposable
    {
        void Commit();
        Task CommitAsync();
    }
}
