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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interneuron.Caching
{
    public interface ICacheProvider
    {
        void Set(string key, object value, TimeSpan cacheDuration);
        void Set<T>(Dictionary<string, T> keyValues, TimeSpan cacheDuration, int batchSize = 30);
        Task SetAsync<T>(Dictionary<string, T> keyValues);
        Task SetAsync<T>(string key, object value, TimeSpan cacheDuration);

        List<T> Get<T>(List<string> keys, int batchSize = 50);
        T Get<T>(string key);
        Task<T> GetAsync<T>(string key);
        Task<List<T>> GetAsync<T>(List<string> keys, int batchSize = 50);
        bool Remove(string key);

        Task<bool> RemoveAsync(string key);

        Task<bool[]> RemoveKeysAsync(List<string> keys);

        void FlushByKeyPattern(string keyPattern);

        Task FlushByKeyPatternAsync(string keyPattern);

        void FlushAll();

        Task FlushAllAsync();

        bool ServerPing();

        Task<bool> ServerPingAsync();

        Task Dispose();

        Task DisposeAndReconnect();
    }
}
