using UnityEngine;

namespace Gamania.GIMChat.Internal.Platform
{
    /// <summary>
    /// Manages GroupChannelCollection syncTime persistence using PlayerPrefs
    ///
    /// Purpose:
    /// - Saves last successful changelog sync timestamp
    /// - On reconnect, syncs from this timestamp to avoid duplicates or gaps
    /// </summary>
    public static class SyncTimeStorage
    {
        /// <summary>
        /// PlayerPrefs key prefix
        /// </summary>
        private const string KeyPrefix = "GimSdk_ChannelCollectionSyncTime_";

        /// <summary>
        /// Saves syncTime in milliseconds since Unix epoch
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="timestampMillis">Timestamp in milliseconds</param>
        public static void SaveSyncTime(string userId, long timestampMillis)
        {
            if (string.IsNullOrEmpty(userId)) return;

            var key = KeyPrefix + userId;
            PlayerPrefs.SetString(key, timestampMillis.ToString());
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Loads syncTime in milliseconds, returns null if not found
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Timestamp in milliseconds or null</returns>
        public static long? LoadSyncTime(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return null;

            var key = KeyPrefix + userId;
            if (!PlayerPrefs.HasKey(key)) return null;

            var value = PlayerPrefs.GetString(key);
            if (long.TryParse(value, out var result))
                return result;

            return null;
        }

        /// <summary>
        /// Clears syncTime (e.g., on user logout)
        /// </summary>
        /// <param name="userId">User ID</param>
        public static void ClearSyncTime(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return;

            var key = KeyPrefix + userId;
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }
    }
}
