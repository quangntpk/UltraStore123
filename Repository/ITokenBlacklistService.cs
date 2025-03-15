using Microsoft.Extensions.Caching.Memory;

namespace UltraStrore.Repository
{
    public interface ITokenBlacklistService
    {
        Task AddTokenToBlacklist(string token, TimeSpan expiry);
        Task<bool> IsTokenBlacklisted(string token);
    }

    public class TokenBlacklistService : ITokenBlacklistService
    {
        private readonly IMemoryCache _cache;

        public TokenBlacklistService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task AddTokenToBlacklist(string token, TimeSpan expiry)
        {
            _cache.Set(token, true, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry
            });
            return Task.CompletedTask;
        }

        public Task<bool> IsTokenBlacklisted(string token)
        {
            return Task.FromResult(_cache.TryGetValue(token, out bool _));
        }
    }
}