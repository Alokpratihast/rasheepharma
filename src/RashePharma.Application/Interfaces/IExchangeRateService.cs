namespace RashePharma.Application.Interfaces;

public interface IExchangeRateService
{
    Task<decimal> GetInrToUsdRateAsync();
}