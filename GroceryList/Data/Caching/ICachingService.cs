using Microsoft.Extensions.Caching.Distributed;

namespace GroceryList.Data.Caching
{
	public interface ICachingService
	{
    public void Wait();
    public void Release();
		Task SetAsync(string key, string value, DistributedCacheEntryOptions? options = null);
		Task<string?> GetAsync(string key);
		void DeleteAsync(string key);
	}
}
