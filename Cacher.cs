using System;
using System.Collections.Generic;

namespace Cacher {
    public class Cacher {
        /// <summary>
        /// Collection of cache entries.
        /// </summary>
        private static Dictionary<string, CacheEntry> Entries { get; set; }

        /// <summary>
        /// Get data from cache and cast it to given type.
        /// </summary>
        /// <typeparam name="T">Type to cast too.</typeparam>
        /// <param name="key">Key to look for.</param>
        /// <param name="callback">Callback function to execute if cache is empty.</param>
        /// <param name="minutes">Amount of minutes to store item in cache.</param>
        /// <param name="slidingExpiration">Whether or not to enable sliding expiration.</param>
        /// <returns>Object</returns>
        public static T Get<T>(string key, Func<object> callback = null, int minutes = 0, bool slidingExpiration = true) {
            if (Entries == null) {
                Entries = new Dictionary<string, CacheEntry>();
            }

            var entry = Entries.ContainsKey(key)
                ? Entries[key]
                : null;

            if (entry != null) {
                if (entry.Expires < DateTime.Now) {
                    entry = null;
                }
                else {
                    if (entry.SlidingExpiration && entry.Minutes > 0) {
                        entry.Expires = DateTime.Now.AddMinutes(entry.Minutes);
                    }
                }
            }

            if (entry == null && callback != null) {
                var value = callback.Invoke();

                if (value != null) {
                    Set(key, value, minutes, slidingExpiration);

                    if (Entries.ContainsKey(key)) {
                        entry = Entries[key];
                    }
                }
            }

            if (entry == null) {
                return default(T);
            }

            try {
                return (T) entry.Data;
            }
            catch {
                return default(T);
            }
        }

        /// <summary>
        /// Save data into cache.
        /// </summary>
        /// <param name="key">Key to store it under.</param>
        /// <param name="value">Data to store.</param>
        /// <param name="minutes">Amount of minutes to store item in cache.</param>
        /// <param name="slidingExpiration">Whether or not to enable sliding expiration.</param>
        public static void Set(string key, object value, int minutes = 0, bool slidingExpiration = true) {
            if (value == null) {
                return;
            }

            if (Entries == null) {
                Entries = new Dictionary<string, CacheEntry>();
            }

            var entry = Entries.ContainsKey(key)
                ? Entries[key]
                : new CacheEntry();

            entry.Minutes = minutes;
            entry.SlidingExpiration = slidingExpiration;
            entry.Data = value;

            if (minutes > 0) {
                entry.Expires = DateTime.Now.AddMinutes(minutes);
            }
            else {
                entry.Expires = null;
            }

            if (!Entries.ContainsKey(key)) {
                Entries.Add(key, entry);
            }
        }

        /// <summary>
        /// Cache entry.
        /// </summary>
        private class CacheEntry {
            /// <summary>
            /// When the data expires, if ever.
            /// </summary>
            public DateTime? Expires { get; set; }

            /// <summary>
            /// Amount of minutes originally added to expiration.
            /// </summary>
            public int Minutes { get; set; }

            /// <summary>
            /// Whether or not the entry uses sliding expiration.
            /// </summary>
            public bool SlidingExpiration { get; set; }

            /// <summary>
            /// Data stored in cache.
            /// </summary>
            public object Data { get; set; }
        }
    }
}