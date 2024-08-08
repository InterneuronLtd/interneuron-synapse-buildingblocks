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
using System.Threading.Tasks;

namespace Interneuron.Caching
{
    public class NullCacheProvider : ICacheProvider
    {
        public void Set(string key, object value, TimeSpan cacheDuration)
        {

        }

        public T Get<T>(string key)
        {
            return default(T);
        }

        public bool Remove(string key)
        {
            return false;
        }

        public void FlushAll()
        {

        }

        public void FlushByKeyPattern(string keyPattern)
        {
            return;
        }

        public List<T> Get<T>(List<string> keys, int batchSize = 50)
        {
            return null;
        }

        public void Set<T>(Dictionary<string, T> keyValues, TimeSpan cacheDuration, int batchSize = 30)
        {
            return;
        }

        public Task<T> GetAsync<T>(string key)
        {
            return default(Task<T>);
        }

        public Task<List<T>> GetAsync<T>(List<string> keys, int batchSize = 50)
        {
            return null;
        }

        public Task SetAsync<T>(Dictionary<string, T> keyValues)
        {
            return null;
        }

        public async Task SetAsync<T>(string key, object value, TimeSpan cacheDuration)
        {
            
        }

        public Task<bool> RemoveAsync(string key)
        {
            return default(Task<bool>);
        }

        public Task<bool[]> RemoveKeysAsync(List<string> keys)
        {
            return default(Task<bool[]>);
        }

        public Task FlushByKeyPatternAsync(string keyPattern)
        {
            return default(Task);
        }

        public Task FlushAllAsync()
        {
            return default(Task);
        }

        public bool ServerPing()
        {
            return false;
        }

        public async Task<bool> ServerPingAsync()
        {
            return await Task.Run(() => false);
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
