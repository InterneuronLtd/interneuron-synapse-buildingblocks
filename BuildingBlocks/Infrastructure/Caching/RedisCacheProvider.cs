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
﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Interneuron.Caching
{
    public class RedisCacheProvider : ICacheProvider
    {
        private ConnectionMultiplexer _conn;
        private IDatabase _cacheInstance = null;
        private CacheSettings _cacheSettings;

        public RedisCacheProvider(CacheSettings cacheSettings)
        {
            _cacheSettings = cacheSettings;

            ConnectToRedis();
        }

        private void ConnectToRedis()
        {
            if (_cacheSettings != null)
            {
                var cacheURL = _cacheSettings.CacheURL;

                var password = _cacheSettings.CacheServerPassword;

                var cacheTimeout = _cacheSettings.CacheConnectionTimeout;

                var configOptions = ConfigurationOptions.Parse(cacheURL);
                //{
                //configOptions.EndPoints = epColl;// { cacheURL },// { "localhost:6379" }
                configOptions.Password = password;
                configOptions.ConnectTimeout = cacheTimeout;
                configOptions.SyncTimeout = cacheTimeout;
                configOptions.AsyncTimeout = cacheTimeout;
                configOptions.ConnectRetry = 5;
                //}
                _conn = ConnectionMultiplexer.Connect(configOptions) ;

                _cacheInstance = _conn.GetDatabase();
            }
        }

        //private static (IDatabase cacheInstance, IServer cacheServer) CreateMemoryCache()
        //{
        //    var cacheURL = cacheSettings.CacheURL;

        //    var conn = ConnectionMultiplexer.Connect(
        //    new ConfigurationOptions
        //    {
        //        EndPoints = { cacheURL }// { "localhost:6379" }
        //    });

        //    var cacheServer = conn.GetServer(cacheURL);//.Keys(conn.GetDatabase(), )

        //    return (conn.GetDatabase(), cacheServer);
        //}

        private void CheckConnectionToRedis()
        {
            if (_cacheSettings != null)
            {
                var cacheURL = _cacheSettings.CacheURL;
                var epColl = new EndPointCollection();
                cacheURL.Split(',').ToList()?.ForEach(rec => epColl.Add(rec));

                var password = _cacheSettings.CacheServerPassword;

                var cacheTimeout = _cacheSettings.CacheConnectionTimeout;

                var configOptions = ConfigurationOptions.Parse(cacheURL);
                
                configOptions.Password = password;
                configOptions.ConnectTimeout = cacheTimeout;
                configOptions.SyncTimeout = cacheTimeout;
                configOptions.AsyncTimeout = cacheTimeout;
                configOptions.ConnectRetry = 5;

                var conn = ConnectionMultiplexer.Connect(configOptions);

                if (conn != null)
                {
                    conn.Close();

                    conn.Dispose();
                }
            }
        }

        public async Task Dispose()
        {
            if (_conn != null)
            {
                await _conn.CloseAsync(true);
                if(_conn != null) _conn.Dispose();
            }
        }

        public async Task DisposeAndReconnect()
        {
            if (_conn != null)
            {
                await _conn.CloseAsync(true);
                if (_conn != null) _conn.Dispose();
            }

            ConnectToRedis();
        }


        public bool ServerPing()
        {
            try
            {
                var eps = _conn.GetEndPoints();
                if (eps == null || eps.Length == 0) return false;

                foreach (var ep in eps)
                {
                    try
                    {
                       _conn.GetServer(ep).Ping();
                        return true;//can be any server
                    }
                    catch { }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ServerPingAsync()
        {

            try
            {
                var eps = _conn.GetEndPoints();
                if (eps == null || eps.Length == 0) return false;

                foreach (var ep in eps)
                {
                    try
                    {
                        await _conn.GetServer(ep).PingAsync();
                        return true;//can be any server
                    }
                    catch { }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }


        public void FlushAll()
        {
            var keys = GetKeys($"*");
            if (keys == null) return;

            foreach (var key in keys)
            {
                _cacheInstance.KeyDelete(key);
            }


            //var keys = _cacheServer.Keys(_cacheInstance.Database, "*");

            //if (keys != null)
            //{
            //    _cacheInstance.KeyDelete(keys.ToArray());
            //}
        }

        public async Task FlushAllAsync()
        {

            await foreach (var key in GetKeysAsync("*"))
            {
                await _cacheInstance.KeyDeleteAsync(key);
            }

            //await foreach (var key in _cacheServer.KeysAsync(_cacheInstance.Database, "*"))
            //{
            //    await _cacheInstance.KeyDeleteAsync(key);
            //}
        }

        public void FlushByKeyPattern(string keyPattern)
        {
            if (string.IsNullOrWhiteSpace(keyPattern)) return;

            var keys = GetKeys($"{keyPattern}*");
            if (keys == null) return;

            foreach (var key in keys)
            {
                _cacheInstance.KeyDelete(key);
            }

            //var keys = _cacheServer.Keys(_cacheInstance.Database, $"{keyPattern}*");

            //if (keys != null)
            //{
            //    _cacheInstance.KeyDelete(keys.ToArray());
            //}
        }

        public async Task FlushByKeyPatternAsync(string keyPattern)
        {
            if (string.IsNullOrWhiteSpace(keyPattern)) return;

            await foreach (var key in GetKeysAsync($"{keyPattern}*"))
            {
                await _cacheInstance.KeyDeleteAsync(key);
            }

            //await foreach(var key in _cacheServer.KeysAsync(_cacheInstance.Database, $"{keyPattern}*"))
            //{
            //    await _cacheInstance.KeyDeleteAsync(key);
            //}
        }

        private async IAsyncEnumerable<string> GetKeysAsync(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(pattern));

            foreach (var endpoint in _conn.GetEndPoints())
            {
                var server = _conn.GetServer(endpoint);
                await foreach (var key in server.KeysAsync(pattern: pattern))
                {
                    yield return key;
                }
            }
        }

        private IEnumerable<string> GetKeys(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(pattern));

            foreach (var endpoint in _conn.GetEndPoints())
            {
                var server = _conn.GetServer(endpoint);

                foreach (var key in server.Keys(pattern: pattern))
                {
                    yield return key;
                }
            }
        }

        public List<T> Get<T>(List<string> keys, int batchSize = 50)
        {
            if (keys == null || keys.Count == 0) return default(List<T>);

            List<RedisKey[]> batches = GetBatches(keys, batchSize);

            var parallelOption = new ParallelOptions() { MaxDegreeOfParallelism = 2 };

            var resultsList = new ConcurrentBag<RedisValue[]>();

            Parallel.ForEach(batches, parallelOption, (batch) =>
            {
                var getKeysData = _cacheInstance.StringGet(batch);

                if (getKeysData != null && getKeysData.Length > 0)
                    resultsList.Add(getKeysData);
            });

            return ProcessResults<T>(resultsList.ToList());
        }

        public async Task<List<T>> GetAsync<T>(List<string> keys, int batchSize = 50)
        {
            if (keys == null || keys.Count == 0) return default(List<T>);

            //List<RedisKey[]> batches = GetBatches(keys, batchSize);

            //var resultsList = new List<RedisValue[]>();

            //Temp fix - since mget is not supported on cluster
            var keyArr = keys.ToArray();
            var resultsList = await Task.WhenAll(Array.ConvertAll(keyArr, (key)=> _cacheInstance.StringGetAsync(key)));

            //foreach (var batch in batches)
            //{
            //    //Temp fix - since mget is not supported on cluster
            //    var getKeysData = await _cacheInstance.StringGetAsync(batch);

            //    if (getKeysData != null && getKeysData.Length > 0)
            //        resultsList.Add(getKeysData);
            //}

            return ProcessResults<T>(resultsList.ToList());
        }

        private List<RedisKey[]> GetBatches(List<string> keys, int batchSize)
        {
            List<RedisKey[]> batches = new List<RedisKey[]>();

            for (int codeIndex = 0; codeIndex < keys.Count; codeIndex += batchSize)
            {
                var batch = keys.Skip(codeIndex).Take(batchSize)?.Select(rec => new RedisKey(rec));
                batches.Add(batch.ToArray());
            }

            return batches;
        }

        private List<T> ProcessResults<T>(List<RedisValue[]> resultsList)
        {
            if (resultsList == null || resultsList.Count == 0) return default(List<T>);
            try
            {
                var flattendResults = resultsList
                    .SelectMany(rec => rec).ToList();

                var results = new List<T>();

                if (flattendResults == null) return null;

                foreach (var item in flattendResults)
                {
                    if (item != RedisValue.Null && item != RedisValue.EmptyString)
                        results.Add(JsonConvert.DeserializeObject<T>(item));
                }

                return results;
            }
            catch (Exception ex)
            { }

            return default(List<T>);
        }

        private List<T> ProcessResults<T>(List<RedisValue> resultsList)
        {
            if (resultsList == null || resultsList.Count == 0) return default(List<T>);
            try
            {
                var results = new List<T>();

                foreach (var item in resultsList)
                {
                    if (item != RedisValue.Null && item != RedisValue.EmptyString)
                        results.Add(JsonConvert.DeserializeObject<T>(item));
                }

                return results;
            }
            catch (Exception ex)
            { }

            return default(List<T>);
        }

        public T Get<T>(string key)
        {
            if(string.IsNullOrEmpty(key)) return default(T);

            var getKeyData = _cacheInstance.StringGet(key);

            try
            {
                return JsonConvert.DeserializeObject<T>(getKeyData);
            }
            catch
            { }
            return default(T);
        }

        public async Task<T> GetAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key)) return default(T);

            var getKeyData = await _cacheInstance.StringGetAsync(key);

            try
            {
                return JsonConvert.DeserializeObject<T>(getKeyData);
            }
            catch
            { }
            return default(T);
        }

        public bool Remove(string key)
        {
            if (key == null) return false;
            return _cacheInstance.KeyDelete(key);
        }

        public async Task<bool> RemoveAsync(string key)
        {
            if (key == null) return false;
            return await _cacheInstance.KeyDeleteAsync(key);
        }

        public async Task<bool[]> RemoveKeysAsync(List<string> keys)
        {
            if (keys == null || keys.Count == 0) return new bool[] { false };
            return await Task.WhenAll(keys.Select(key => _cacheInstance.KeyDeleteAsync(key)));
        }

        public void Set(string key, object value, TimeSpan cacheDuration)
        {
            if (string.IsNullOrEmpty(key) || value == null) return;

            string obj = null;
            try
            {
                obj = JsonConvert.SerializeObject(value);
            }
            catch { return; }

            _cacheInstance.StringSet(key, obj);

        }

        public void Set<T>(Dictionary<string, T> keyValues, TimeSpan cacheDuration, int batchSize = 30)
        {
            if (keyValues == null || keyValues.Count == 0) return;

            var redisKeys = new List<RedisKey>();

            List<KeyValuePair<RedisKey, RedisValue>[]> batches = new List<KeyValuePair<RedisKey, RedisValue>[]>();

            for (int codeIndex = 0; codeIndex < keyValues.Count; codeIndex += batchSize)
            {
                var batch = keyValues.Skip(codeIndex).Take(batchSize)?.ToList();

                if(batch == null || batch.Count > 0)
                {
                    var kvList = new List<KeyValuePair<RedisKey, RedisValue>>();

                    foreach (var batchItem in batch)
                    {
                        if(batchItem.Value != null && batchItem.Key != null)
                        {
                            try
                            {
                                var val = JsonConvert.SerializeObject(batchItem.Value);
                                var kv = new KeyValuePair<RedisKey, RedisValue>(new RedisKey(batchItem.Key), new RedisValue(val));
                                kvList.Add(kv);
                            }
                            catch { return; }
                        }
                    }

                    batches.Add(kvList.ToArray());
                }
            }

            var parallelOption = new ParallelOptions() { MaxDegreeOfParallelism = 2 };
            Parallel.ForEach(batches, parallelOption, (batch) =>
            {
                _cacheInstance.StringSet(batch);
            });
        }

        public async Task SetAsync<T>(string key, object value, TimeSpan cacheDuration)
        {
            if (string.IsNullOrEmpty(key)) return;

            string val = null;

            try
            {
                val = JsonConvert.SerializeObject(value);
            }
            catch { }
            if (val == null) val = "";

            await _cacheInstance.StringSetAsync(key, val);
        }

        public async Task SetAsync<T>(Dictionary<string, T> keyValues)
        {
            if (keyValues == null || keyValues.Count == 0) return;

            await Task.WhenAll(keyValues.Select((kv) =>
            {
                string val = null;
                try
                {
                    val = JsonConvert.SerializeObject(kv.Value);
                }
                catch {}
                if (val == null) val = "";

                return _cacheInstance.StringSetAsync(kv.Key, val);
            }));
        }

        private List<string> GetHashedTaggedKey(List<string> keys)
        {
            var hashTaggedKeys = new List<string>();

            foreach (var key in keys)
            {
                //Pass to hash fn
                if (string.IsNullOrEmpty(key))
                    hashTaggedKeys.Add("{"+_conn.HashSlot(key)+"}:"+ key);
            }
            return hashTaggedKeys;
        }

        private string GetHashTaggedKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            return "{" + _conn.HashSlot(key) + "}:" + key;
        }

    }
}

