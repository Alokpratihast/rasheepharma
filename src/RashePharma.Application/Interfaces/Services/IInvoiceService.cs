namespace RashePharma.Application.Interfaces.Services;

public interface IInvoiceService
{
    Task<byte[]> GenerateInvoiceAsync(int orderId);
}