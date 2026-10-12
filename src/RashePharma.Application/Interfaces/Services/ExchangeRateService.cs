using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using RashePharma.Application.Interfaces;

namespace RashePharma.Application.Services;

public class ExchangeRateService : IExchangeRateService
{
    private const string InrToUsdCacheKey = "exchange_rate_inr_to_usd";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    private readonly IMemoryCache _cache;
    private readonly HttpClient _httpClient;

    public ExchangeRateService(
        IMemoryCache cache,
        HttpClient httpClient)
    {
        _cache = cache;
        _httpClient = httpClient;
    }

    public async Task<decimal> GetInrToUsdRateAsync()
    {
        if (_cache.TryGetValue(InrToUsdCacheKey, out decimal cachedRate))
        {
            return cachedRate;
        }

        var response = await _httpClient.GetFromJsonAsync<ExchangeRateResponse>(
            "https://api.frankfurter.dev/v2/rate/inr/usd");

        if (response is null || response.Rate <= 0)
        {
            throw new InvalidOperationException(
                "Unable to get a valid INR to USD exchange rate.");
        }

        _cache.Set(
            InrToUsdCacheKey,
            response.Rate,
            CacheDuration);

        return response.Rate;
    }

    private sealed class ExchangeRateResponse
    {
        public string Date { get; set; } = string.Empty;
        public string Base { get; set; } = string.Empty;
        public string Quote { get; set; } = string.Empty;
        public decimal Rate { get; set; }
    }
}