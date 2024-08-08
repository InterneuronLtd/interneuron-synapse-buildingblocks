 //Interneuron synapse

//Copyright(C) 2024 Interneuron Limited

//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

//See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program.If not, see<http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Text;
using System.Threading.Tasks;

namespace Interneuron.Caching
{
    public class CacheService
    {
        static readonly Lazy<ICacheProvider> CacheProvider = new Lazy<ICacheProvider>(GetCacheProvider, true);

        public static Func<ICacheProvider> CacheProviderDelegate;

        private static TimeSpan defaultCacheDuration = TimeSpan.MinValue;

        public static CacheSettings CacheSettings;

        static TimeSpan DefaultCacheDuration
        {
            get
            {
                if(defaultCacheDuration == TimeSpan.MinValue)
                {
                    var cacheMinutes = CacheSettings.CacheDurationInMinutes;
                    defaultCacheDuration = TimeSpan.FromMinutes(cacheMinutes);
                }
                return defaultCacheDuration;
            }
        }

        static ICacheProvider Cache { get { return CacheProvider.Value; } }

        static CacheService()
        {
            CacheProviderDelegate = BuildCacheProvider;
        }

        static ICacheProvider GetCacheProvider()
        {
            return CacheProviderDelegate();
        }

        private static ICacheProvider BuildCacheProvider()
        {
            var cacheEnabled = CacheSettings.Enabled;

            if(cacheEnabled)
            {
                var cacheProviderType = CacheSettings.Provider;

                if (!string.IsNullOrWhiteSpace(cacheProviderType))
                {
                    var type = Type.GetType(cacheProviderType); 
                    
                    if(type == null)
                        throw new Exception(string.Format("Unable to find caching provider of type : {0}",cacheProviderType));

                    return BuildCacheProvider(type);
                }

                return new InMemoryCacheProvider(CacheSettings);
            }

            return new NullCacheProvider();
        }

        private static ICacheProvider BuildCacheProvider(Type provider)
        {
            var obj = Activator.CreateInstance(provider, CacheSettings) as ICacheProvider;
            if (obj == null)
            {
                throw new InvalidOperationException("Unable to create cache provider from type " + provider.FullName);
            }
            return obj;
        }

        public static void Set(string key, object value)
        {
            Cache.Set(key,value,DefaultCacheDuration);
        }

        public static void Set(string key, object value, TimeSpan cacheDuration)
        {
            Cache.Set(key, value, cacheDuration);
        }

        public static void Set<T>(Dictionary<string, T> keyValues, int batchSize = 30)
        {
            Cache.Set<T>(keyValues, DefaultCacheDuration, batchSize);
        }

        public static async Task SetAsync<T>(Dictionary<string, T> keyValues, int batchSize = 30)
        {
            await Cache.SetAsync<T>(keyValues);
        }

        public static async Task SetAsync<T>(string key, T value)
        {
            await Cache.SetAsync<T>(key, value, DefaultCacheDuration);
        }

        public static T Get<T>(string key)
        {
            return Cache.Get<T>(key);
        }

        public static async Task<T> GetAsync<T>(string key)
        {
            return await Cache.GetAsync<T>(key);
        }

        public static List<T> Get<T>(List<string> keys, int batchSize = 50)
        {
            return Cache.Get<T>(keys, batchSize);
        }

        public static async Task<List<T>> GetAsync<T>(List<string> keys, int batchSize = 50)
        {
            return await Cache.GetAsync<T>(keys, batchSize);
        }

        public static bool Remove(string key)
        {
            return Cache.Remove(key);
        }

        public static async Task<bool> RemoveAsync(string key)
        {
            if (key == null) return false;
            return await Cache.RemoveAsync(key);
        }

        public static async Task<bool[]> RemoveKeysAsync(List<string> keys)
        {
            if (keys == null || keys.Count == 0) return new bool[] { false };
            return await Cache.RemoveKeysAsync(keys);
        }

        public static void FlushAll()
        {
            Cache.FlushAll();
        }

        public static async Task FlushAllAsync()
        {
            await Cache.FlushAllAsync();
        }

        public static void FlushByKeyPattern(string key)
        {
            if (key == null) return;

            Cache.FlushByKeyPattern(key);
        }

        public static async Task FlushByKeyPatternAsync(string key)
        {
            if (key == null) return;

            await Cache.FlushByKeyPatternAsync(key);
        }

        public static bool ServerPing()
        {
            try
            {
                return Cache.ServerPing();
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> ServerPingAsync()
        {
            try
            {
                return await Cache.ServerPingAsync();
            }
            catch
            {
                return false;
            }
        }

        public static async Task Dispose()
        {
            await Cache.Dispose();
        }

        public static async Task DisposeAndReconnect()
        {
            await Cache.DisposeAndReconnect();
        }
    }
}
