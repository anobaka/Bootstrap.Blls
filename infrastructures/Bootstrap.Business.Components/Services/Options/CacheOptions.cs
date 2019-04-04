using System;

namespace Bootstrap.Business.Components.Services.Options
{
    /// <summary>
    /// TODO: Do not use this for now.
    /// </summary>
    public class CacheOptions
    {
        public TimeSpan CacheTime { get; set; }
        public string CacheKey { get; set; }
        public bool Refresh { get; set; }
        private const string CacheKeySeparator = "-";

        public bool IsValid => CacheTime.TotalSeconds > 0 && !string.IsNullOrEmpty(CacheKey);

        public CacheOptions(TimeSpan cacheTime, string cacheKey)
        {
            CacheTime = cacheTime;
            CacheKey = cacheKey;
        }

        public CacheOptions(TimeSpan cacheTime, params object[] keyParameters) : this(cacheTime,
            string.Join(CacheKeySeparator, keyParameters))
        {
        }

        public static CacheOptions BuildForMinutes(int minutes, string key) =>
            new CacheOptions(TimeSpan.FromMinutes(minutes), key);

        public static CacheOptions BuildForMinutes(int minutes, params object[] keyParameters) =>
            new CacheOptions(TimeSpan.FromMinutes(minutes), keyParameters);
    }

}