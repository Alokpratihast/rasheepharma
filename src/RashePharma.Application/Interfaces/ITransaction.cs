namespace RashePharma.Application.Interfaces;

public interface ITransaction : IAsyncDisposable
{
    Task CommitAsync();

    Task RollbackAsync();
}