using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Gamania.GIMChat.Internal.Data.Cache;
using Gamania.GIMChat.Internal.Data.DTOs;
using Gamania.GIMChat.Internal.Data.Mappers;
using Gamania.GIMChat.Internal.Data.Network;
using Gamania.GIMChat.Internal.Domain.Models;
using Gamania.GIMChat.Internal.Domain.Repositories;

namespace Gamania.GIMChat.Internal.Data.Repositories
{
    /// <summary>
    /// Default implementation of channel repository with caching support
    /// </summary>
    public class ChannelRepositoryImpl : IChannelRepository
    {
        private readonly IHttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly ChannelCache _cache;
        private readonly bool _cacheEnabled;

        public ChannelRepositoryImpl(
            IHttpClient httpClient,
            string baseUrl,
            bool enableCache = true,
            ChannelCache cache = null)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
            _cacheEnabled = enableCache;
            _cache = cache ?? new ChannelCache();
        }

        public async Task<ChannelBO> GetChannelAsync(
            string channelUrl,
            CancellationToken cancellationToken = default)
        {
            if (_cacheEnabled && _cache.TryGet(channelUrl, out var cachedChannel))
            {
                return cachedChannel;
            }

            return await ExecuteAsync(async () =>
            {
                var url = BuildChannelUrl(channelUrl);
                var response = await _httpClient.GetAsync(url, cancellationToken: cancellationToken);

                if (!response.IsSuccess)
                {
                    throw CreateExceptionFromResponse(response);
                }

                return ProcessAndCacheChannelResponse(response);
            }, "Failed to get channel", channelUrl);
        }

        public async Task<ChannelBO> CreateChannelAsync(
            GimGroupChannelCreateParams createParams,
            CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(async () =>
            {
                var url = BuildChannelsEndpoint();
                var requestBody = JsonConvert.SerializeObject(new
                {
                    user_ids = createParams.UserIds,
                    operator_user_ids = createParams.OperatorUserIds,
                    name = createParams.Name,
                    cover_url = createParams.CoverUrl,
                    custom_type = createParams.CustomType,
                    data = createParams.Data,
                    is_distinct = createParams.IsDistinct
                });

                var response = await _httpClient.PostAsync(url, requestBody, cancellationToken: cancellationToken);

                if (!response.IsSuccess)
                {
                    throw CreateExceptionFromResponse(response);
                }

                return ProcessAndCacheChannelResponse(response);
            }, "Failed to create channel");
        }

        public async Task<ChannelBO> UpdateChannelAsync(
            string channelUrl,
            GimGroupChannelUpdateParams updateParams,
            CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(async () =>
            {
                var url = BuildChannelUrl(channelUrl);
                var requestBody = JsonConvert.SerializeObject(new
                {
                    name = updateParams.Name,
                    cover_url = updateParams.CoverUrl,
                    custom_type = updateParams.CustomType,
                    data = updateParams.Data
                });

                var response = await _httpClient.PutAsync(url, requestBody, cancellationToken: cancellationToken);

                if (!response.IsSuccess)
                {
                    throw CreateExceptionFromResponse(response);
                }

                return ProcessAndCacheChannelResponse(response);
            }, "Failed to update channel", channelUrl);
        }

        public async Task DeleteChannelAsync(
            string channelUrl,
            CancellationToken cancellationToken = default)
        {
            await ExecuteAsync(async () =>
            {
                var url = BuildChannelUrl(channelUrl);
                var response = await _httpClient.DeleteAsync(url, cancellationToken: cancellationToken);

                if (!response.IsSuccess)
                {
                    throw CreateExceptionFromResponse(response);
                }

                if (_cacheEnabled)
                {
                    _cache.Remove(channelUrl);
                }
            }, "Failed to delete channel", channelUrl);
        }

        public async Task<ChannelBO> InviteUsersAsync(
            string channelUrl,
            string[] userIds,
            CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(async () =>
            {
                var url = $"{BuildChannelUrl(channelUrl)}/invite";
                var requestBody = JsonConvert.SerializeObject(new { user_ids = userIds });

                var response = await _httpClient.PostAsync(url, requestBody, cancellationToken: cancellationToken);

                if (!response.IsSuccess)
                {
                    throw CreateExceptionFromResponse(response);
                }

                return ProcessAndCacheChannelResponse(response);
            }, "Failed to invite users", channelUrl);
        }

        public async Task<ChannelListResult> ListChannelsAsync(
            string userId,
            int limit = 20,
            string token = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("userId is required for ListChannels", nameof(userId));

            return await ExecuteAsync(async () =>
            {
                var path = $"{_baseUrl}/users/{userId}/my_group_channels";
                var query = $"?limit={limit}";
                if (!string.IsNullOrEmpty(token))
                    query += $"&token={Uri.EscapeDataString(token)}";
                var url = path + query;

                var response = await _httpClient.GetAsync(url, cancellationToken: cancellationToken);

                if (!response.IsSuccess)
                {
                    throw CreateExceptionFromResponse(response);
                }

                var dto = JsonConvert.DeserializeObject<ChannelListResponseDTO>(response.Body);
                var channels = (dto?.channels ?? new List<ChannelDTO>())
                    .Select(ChannelDtoMapper.ToBusinessObject)
                    .Where(c => c != null)
                    .ToList();

                if (_cacheEnabled)
                {
                    foreach (var ch in channels)
                    {
                        if (!string.IsNullOrWhiteSpace(ch.ChannelUrl))
                            _cache.Set(ch.ChannelUrl, ch);
                    }
                }

                return new ChannelListResult
                {
                    Channels = channels.AsReadOnly(),
                    NextToken = string.IsNullOrEmpty(dto?.next) ? null : dto.next
                };
            }, "Failed to list channels", userId);
        }

        public async Task<ChannelChangeLogResult> GetChangeLogsAsync(
            string userId,
            long syncTimestamp,
            string token = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("userId is required for GetChangeLogs", nameof(userId));

            return await ExecuteAsync(async () =>
            {
                var path = $"{_baseUrl}/users/{userId}/my_group_channels/changelogs";
                var query = $"?change_ts={syncTimestamp}";
                if (!string.IsNullOrEmpty(token))
                    query += $"&token={Uri.EscapeDataString(token)}";
                
                query += "&show_member=true&show_read_receipt=true&show_delivery_receipt=true";
                var url = path + query;

                var response = await _httpClient.GetAsync(url, cancellationToken: cancellationToken);

                if (!response.IsSuccess)
                {
                    throw CreateExceptionFromResponse(response);
                }

                var dto = JsonConvert.DeserializeObject<ChannelChangeLogResponseDTO>(response.Body);

                // Map updated channels to business objects
                var updatedChannels = (dto?.updated ?? new List<ChannelDTO>())
                    .Select(ChannelDtoMapper.ToBusinessObject)
                    .Where(c => c != null)
                    .ToList();

                // Cache updated channels
                if (_cacheEnabled)
                {
                    foreach (var ch in updatedChannels)
                    {
                        if (!string.IsNullOrWhiteSpace(ch.ChannelUrl))
                            _cache.Set(ch.ChannelUrl, ch);
                    }
                }

                // Map deleted channels
                var deletedUrls = dto?.deleted ?? new List<string>();

                return new ChannelChangeLogResult
                {
                    UpdatedChannels = updatedChannels.AsReadOnly(),
                    DeletedChannelUrls = deletedUrls.AsReadOnly(),
                    NextToken = string.IsNullOrEmpty(dto?.next) ? null : dto.next
                };
            }, "Failed to get change logs", userId);
        }

        private async Task<T> ExecuteAsync<T>(
            Func<Task<T>> operation,
            string errorMessage,
            string context = null)
        {
            try
            {
                return await operation();
            }
            catch (GimException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new GimException(GimErrorCode.NetworkError, errorMessage, context, ex);
            }
        }

        private async Task ExecuteAsync(
            Func<Task> operation,
            string errorMessage,
            string context = null)
        {
            try
            {
                await operation();
            }
            catch (GimException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new GimException(
                    GimErrorCode.NetworkError,
                    errorMessage,
                    context,
                    ex
                );
            }
        }
        
        private GimException CreateExceptionFromResponse(HttpResponse response)
        {
            return GimException.FromHttpResponse(response);
        }

        private string BuildChannelUrl(string channelUrl)
        {
            return $"{_baseUrl}/group_channels/{channelUrl}";
        }

        private string BuildChannelsEndpoint()
        {
            return $"{_baseUrl}/group_channels";
        }

        private ChannelBO ProcessChannelResponse(HttpResponse response)
        {
            var channelDto = JsonConvert.DeserializeObject<ChannelDTO>(response.Body);
            return ChannelDtoMapper.ToBusinessObject(channelDto);
        }

        /// <summary>
        /// Always cache using canonical channelUrl from response
        /// </summary>
        private ChannelBO ProcessAndCacheChannelResponse(HttpResponse response)
        {
            var channel = ProcessChannelResponse(response);

            if (_cacheEnabled && !string.IsNullOrWhiteSpace(channel.ChannelUrl))
            {
                _cache.Set(channel.ChannelUrl, channel);
            }

            return channel;
        }
    }
}