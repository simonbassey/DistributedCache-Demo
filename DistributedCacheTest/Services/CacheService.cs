using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
namespace DistributedCacheTest.Services
{
    public interface ICacheService
    {
        Task<bool> AddEntry<T>(string key, T value, TimeSpan absoluteExpiry, TimeSpan slidingExpiry);
        Task<bool> KeyExist(string key);
        Task<T> GetEntry<T>(string key) where T : class;
        Task<bool> RemoveEntry(string key);
        Task<bool> RefreshEntry(string key);

    }

    public class CacheService : ICacheService
    {
        readonly IDistributedCache _distributedCache;
        public CacheService(IDistributedCache cache)
        {
            _distributedCache = cache;
        }

        public async Task<bool> AddEntry<T>(string key, T value, TimeSpan absoluteExpiry, TimeSpan slidingExpiry)
        {
            try
            {
                var entry = await _distributedCache.GetStringAsync(key);
                if (!string.IsNullOrEmpty(entry))
                    await _distributedCache.RemoveAsync(key);
                await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(value), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = absoluteExpiry,
                    SlidingExpiration = slidingExpiry
                });
                return !string.IsNullOrEmpty((await _distributedCache.GetStringAsync(key)));
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<T> GetEntry<T>(string key) where T : class
        {
            try
            {
                var entry = await _distributedCache.GetStringAsync(key);
                if (string.IsNullOrEmpty(entry))
                    return null;
                return JsonConvert.DeserializeObject<T>(entry);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> KeyExist(string key)
        {
            try
            {
                var entry = await _distributedCache.GetStringAsync(key);
                if (string.IsNullOrEmpty(entry))
                    return false;
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> RefreshEntry(string key)
        {
            try
            {
                var keyExist = await KeyExist(key);
                if (!keyExist)
                    return false;
                await _distributedCache.RefreshAsync(key);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> RemoveEntry(string key)
        {
            try
            {
                await _distributedCache.RemoveAsync(key);
                return string.IsNullOrEmpty((await _distributedCache.GetStringAsync(key)));
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
