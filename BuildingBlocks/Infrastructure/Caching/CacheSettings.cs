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
using Microsoft.Extensions.Configuration;
using System;

namespace Interneuron.Caching
{
    public class CacheSettings
    {
        private IConfiguration _configuration;

        static class Constants
        {
            public const string CacheDurationInMinutes = "cacheDurationInMinutes";
            public const string Enabled = "enabled";
            public const string Provider = "provider";
            public const string CacheURL = "cacheUrl";
            public const string CacheServerPassword = "cacheServerPassword";
            public const string CacheConnectionTimeout = "cacheConnectionTimeout";
        }

        //private static readonly CacheSettings Settings = CacheServiceExtension.Configuration == null ? null : CacheServiceExtension.Configuration.GetSection("cache").Get<CacheSettings>();

        //public static CacheSettings Instance
        //{
        //    get
        //    {
        //        return Settings ?? new CacheSettings();
        //    }
        //}




        public CacheSettings(IConfiguration configuration)
        {
            this._configuration = configuration;

            var cacheSection = this._configuration.GetSection("cache");

            if (cacheSection.Exists())
            {
                var duration = cacheSection.GetValue<int>(Constants.CacheDurationInMinutes);
                this.CacheDurationInMinutes = duration;

                var enabled = cacheSection.GetValue<bool>(Constants.Enabled);
                this.Enabled = enabled;

                var provider = cacheSection.GetValue<string>(Constants.Provider);
                this.Provider = provider;

                var cacheUrl = cacheSection.GetValue<string>(Constants.CacheURL);
                this.CacheURL = cacheUrl;

                var cacheServerPassword = cacheSection.GetValue<string>(Constants.CacheServerPassword);
                this.CacheServerPassword = cacheServerPassword;

                var cacheTimeout = cacheSection.GetValue<int>(Constants.CacheConnectionTimeout);
                this.CacheConnectionTimeout = cacheTimeout;
            }
        }


        public int CacheDurationInMinutes { get; } = 5;

        public string Provider { get; set; } = null;

        public bool Enabled { get; set; } = false;

        public string CacheURL { get; set; } = null;

        public string CacheServerPassword { get; set; } = null;

        public int CacheConnectionTimeout { get; set; } = 1000;
    }
}
