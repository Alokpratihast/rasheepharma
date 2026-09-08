namespace RashePharma.Application.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync();

    Task<ITransaction> BeginTransactionAsync();
}