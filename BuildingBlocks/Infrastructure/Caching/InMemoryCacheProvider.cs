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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace Interneuron.Caching
{
    public class InMemoryCacheProvider : ICacheProvider
    {
        static MemoryCache cacheInstance = CreateMemoryCache();
        private CacheSettings _cacheSettings;

        public InMemoryCacheProvider()
        {

        }

        public InMemoryCacheProvider(CacheSettings cacheSettings)
        {
            _cacheSettings = cacheSettings;
        }

        private static MemoryCache CreateMemoryCache()
        {
            return new MemoryCache(AppDomain.CurrentDomain.FriendlyName);
        }

        public void Set(string key, object value, TimeSpan cacheDuration)
        {
            cacheInstance.Set(key, value, DateTimeOffset.Now.Add(cacheDuration));
        }

        public T Get<T>(string key)
        {
            object val = cacheInstance.Get(key);
            return val == null ? default(T) : (T)val;
        }

        public bool Remove(string key)
        {
            return cacheInstance.Remove(key) != null;
        }

        public void FlushAll()
        {
            cacheInstance.Dispose();
            cacheInstance = CreateMemoryCache();
        }

        public async Task FlushAllAsync()
        {
            await Task.Run(() => FlushAll());
        }

        //This is not supported
        public void FlushByKeyPattern(string keyPattern)
        {
            return;
        }

        public List<T> Get<T>(List<string> keys, int batchSize = 50)
        {
            if (keys == null || keys.Count == 0) return default(List<T>);


            var parallelOption = new ParallelOptions() { MaxDegreeOfParallelism = 2 };

            var resultsList = new ConcurrentBag<T>();

            Parallel.ForEach(keys, parallelOption, (key) =>
            {
                var getKeysData = Get<T>(key);

                if (getKeysData != null)
                    resultsList.Add(getKeysData);
            });

            return resultsList.ToList();
        }

        public void Set<T>(Dictionary<string, T> keyValues, TimeSpan cacheDuration, int batchSize = 30)
        {
            if (keyValues == null || keyValues.Count == 0) return;

            var parallelOption = new ParallelOptions() { MaxDegreeOfParallelism = 2 };

            Parallel.ForEach(keyValues, parallelOption, (kv) =>
            {
                if (!string.IsNullOrEmpty(kv.Key))
                    Set(kv.Key, kv.Value, cacheDuration);
            });

        }

        public async Task<T> GetAsync<T>(string key)
        {
            return await Task.Run<T>(() => Get<T>(key));
        }

        public async Task<List<T>> GetAsync<T>(List<string> keys, int batchSize = 50)
        {
            return await Task.Run<List<T>>(() => Get<T>(keys, batchSize));
        }

        public async Task SetAsync<T>(Dictionary<string, T> keyValues)
        {
            if (keyValues == null || keyValues.Count == 0) return;

            var cacheDurationInMins = TimeSpan.FromMinutes(600000);

            if (_cacheSettings != null)
                cacheDurationInMins = TimeSpan.FromMinutes(_cacheSettings.CacheDurationInMinutes);

            await Task.WhenAll(keyValues.Select(kv=> Task.Run(() => Set(kv.Key, kv.Value, cacheDurationInMins))));
        }

        public async Task SetAsync<T>(string key, object value, TimeSpan cacheDuration)
        {
            if (string.IsNullOrEmpty(key)) return;

            await Task.Run(() => Set(key, value, cacheDuration));
        }

        public async Task<bool> RemoveAsync(string key)
        {
            if (key == null) return false;
            return await Task.Run(() => Remove(key));
        }

        public async Task<bool[]> RemoveKeysAsync(List<string> keys)
        {
            if (keys == null || keys.Count == 0) return new bool[] { false };
            return await Task.WhenAll(keys.Select(key => RemoveAsync(key)));
        }

        public Task FlushByKeyPatternAsync(string keyPattern)
        {
            return Task.CompletedTask;
        }

        public bool ServerPing()
        {
            return true;
        }

        public async Task<bool> ServerPingAsync()
        {
            return await Task.Run(()=> ServerPing());
        }

        public async Task Dispose()
        {
            await Task.CompletedTask;
        }

        public async Task DisposeAndReconnect()
        {
            await Task.CompletedTask;
        }
    }
}
