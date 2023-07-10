using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using Microsoft.Extensions.Caching.Distributed;

namespace GroceryList.Data.Caching
{
	public class CachingService : ICachingService
	{
		private readonly IDistributedCache _cache;
		private DistributedCacheEntryOptions _options;
		private bool _isCachingOn;
    private static SemaphoreSlim _cacheLock = new SemaphoreSlim(1);

    public void Wait()
    {
      if(_isCachingOn) _cacheLock.Wait();
    }

    public void Release()
    {
      if(_isCachingOn) _cacheLock.Release();
    }

		public CachingService(IDistributedCache cache, IConfiguration config)
		{
			_isCachingOn = config.GetValue<bool>("RedisCache:RedisIsOn");
			_cache = cache;
			_options = new DistributedCacheEntryOptions
			{
				//AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
				//SlidingExpiration = TimeSpan.FromSeconds(60),
			};
		}

		public async Task<string?> GetAsync(string key)
		{
			if(_isCachingOn)
				return await _cache.GetStringAsync(key);
			else
				return null;
		}

		public async Task SetAsync(string key, string value, DistributedCacheEntryOptions? options = null)
		{
			if(_isCachingOn) await _cache.SetStringAsync(key, value, options ?? _options);
		}

		public async void DeleteAsync(string key)
		{
			if(_isCachingOn) await _cache.RemoveAsync(key);
		}
	}
}
