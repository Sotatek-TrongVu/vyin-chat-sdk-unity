using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gamania.GIMChat.Internal.Domain.Log;
using Gamania.GIMChat.Internal.Domain.Mappers;
using Gamania.GIMChat.Internal.Domain.Models;
using Gamania.GIMChat.Internal.Domain.Repositories;
using Gamania.GIMChat.Internal.Platform.Unity;
using Logger = Gamania.GIMChat.Internal.Domain.Log.Logger;

namespace Gamania.GIMChat
{
    /// <summary>
    /// Query for fetching group channel list with pagination.
    /// Owns token, limit, and load logic.
    /// Create via <see cref="Create"/> or <see cref="Create(int)"/>.
    /// </summary>
    public class GimGroupChannelListQuery
    {
        private string _token;

        /// <summary>Max channels per page. Default 20.</summary>
        public int Limit { get; }

        /// <summary>Whether more data can be fetched. Derived from token.</summary>
        public bool HasNext { get; private set; } = true;

        /// <summary>Whether a load is in progress.</summary>
        public bool IsLoading { get; private set; }

        private GimGroupChannelListQuery(int limit)
        {
            Limit = limit > 0 ? limit : 20;
        }

        /// <summary>
        /// Creates a query with default limit (20).
        /// </summary>
        public static GimGroupChannelListQuery Create() => Create(20);

        /// <summary>
        /// Creates a query with specified limit.
        /// </summary>
        public static GimGroupChannelListQuery Create(int limit) => new GimGroupChannelListQuery(limit);

        /// <summary>
        /// Loads the next page. Token is stored in this query and updated from API response.
        /// </summary>
        public async Task<IReadOnlyList<GimGroupChannel>> LoadNextAsync()
        {
            if (!HasNext)
            {
                Logger.Info(LogCategory.Channel, "No more channels to load.");
                return new List<GimGroupChannel>().AsReadOnly();
            }
            if (IsLoading)
            {
                Logger.Info(LogCategory.Channel, "Load already in progress.");
                return new List<GimGroupChannel>().AsReadOnly();
            }

            var userId = GIMChat.CurrentUser?.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                throw new GimException(GimErrorCode.InvalidInitialization,
                    "User must be connected to load channel list. Call GIMChat.Connect() first.");
            }

            IsLoading = true;
            try
            {
                Logger.Info(LogCategory.Channel, "Loading channels...");
                var repo = GIMChatMain.Instance.GetChannelRepository();
                var result = await repo.ListChannelsAsync(userId, Limit, string.IsNullOrEmpty(_token) ? null : _token);

                var channelList = result?.Channels ?? (IReadOnlyList<ChannelBO>)new List<ChannelBO>();
                var vcChannels = channelList.Select(ChannelBoMapper.ToPublicModel).Where(c => c != null).ToList();

                _token = result?.NextToken;
                HasNext = !string.IsNullOrEmpty(_token) || channelList.Count >= Limit;

                return vcChannels.AsReadOnly();
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
