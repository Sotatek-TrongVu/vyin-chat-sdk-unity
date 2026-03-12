using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Gamania.GIMChat.Internal.Domain.Models;

namespace Gamania.GIMChat.Internal.Domain.Repositories
{
    /// <summary>
    /// Result of a channel list pagination request.
    /// </summary>
    public class ChannelListResult
    {
        public IReadOnlyList<ChannelBO> Channels { get; set; }
        public string NextToken { get; set; }
    }

    /// <summary>
    /// Result of a channel changelog request
    ///
    /// Used to sync channel changes missed during offline period
    /// </summary>
    public class ChannelChangeLogResult
    {
        /// <summary>
        /// Updated channels (includes newly added and modified channels)
        /// </summary>
        public IReadOnlyList<ChannelBO> UpdatedChannels { get; set; }

        /// <summary>
        /// List of deleted channel URLs
        /// </summary>
        public IReadOnlyList<string> DeletedChannelUrls { get; set; }

        /// <summary>
        /// Next page token (null if no more pages)
        /// </summary>
        public string NextToken { get; set; }
    }

    /// <summary>
    /// Repository interface for channel data access operations
    /// </summary>
    public interface IChannelRepository
    {
        /// <summary>
        /// Retrieves a channel by its URL
        /// </summary>
        /// <param name="channelUrl">The URL of the channel to retrieve</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The channel data</returns>
        Task<ChannelBO> GetChannelAsync(
            string channelUrl,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new group channel
        /// </summary>
        /// <param name="createParams">Parameters for channel creation</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The created channel data</returns>
        Task<ChannelBO> CreateChannelAsync(
            GimGroupChannelCreateParams createParams,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing channel's properties
        /// </summary>
        /// <param name="channelUrl">The URL of the channel to update</param>
        /// <param name="updateParams">Parameters for channel update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The updated channel data</returns>
        Task<ChannelBO> UpdateChannelAsync(
            string channelUrl,
            GimGroupChannelUpdateParams updateParams,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a channel
        /// </summary>
        /// <param name="channelUrl">The URL of the channel to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task DeleteChannelAsync(
            string channelUrl,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Invites users to join a channel
        /// </summary>
        /// <param name="channelUrl">The URL of the channel</param>
        /// <param name="userIds">Array of user IDs to invite</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The updated channel data</returns>
        Task<ChannelBO> InviteUsersAsync(
            string channelUrl,
            string[] userIds,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists group channels for the given user with pagination.
        /// </summary>
        /// <param name="userId">User ID for list (typically current user)</param>
        /// <param name="limit">Max channels per page (default 20)</param>
        /// <param name="token">Next page token, null for first page</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Channels and next token (null when no more pages)</returns>
        Task<ChannelListResult> ListChannelsAsync(
            string userId,
            int limit = 20,
            string token = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves channel changelog for offline sync
        ///
        /// Retrieves all added, updated, and deleted channels since the specified timestamp
        /// </summary>
        /// <param name="userId">User ID (typically current user)</param>
        /// <param name="syncTimestamp">Sync timestamp in milliseconds since epoch</param>
        /// <param name="token">Pagination token (default null)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Changelog result (updated channels, deleted channel URLs, next token)</returns>
        Task<ChannelChangeLogResult> GetChangeLogsAsync(
            string userId,
            long syncTimestamp,
            string token = null,
            CancellationToken cancellationToken = default);
    }
}
