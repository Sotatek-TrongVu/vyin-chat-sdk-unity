using Gamania.GIMChat.Internal.Data.DTOs;
using Gamania.GIMChat.Internal.Domain.Models;

namespace Gamania.GIMChat.Internal.Data.Mappers
{
    /// <summary>
    /// Mapper for converting between ChannelDTO and ChannelBO
    /// Data Layer responsibility: DTO ↔ Business Object conversion
    /// </summary>
    public static class ChannelDtoMapper
    {
        /// <summary>
        /// Convert ChannelDTO to ChannelBO (Business Object)
        /// </summary>
        public static ChannelBO ToBusinessObject(ChannelDTO dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new ChannelBO
            {
                ChannelUrl = dto.channel_url,
                Name = dto.name,
                CoverUrl = dto.cover_url,
                CustomType = dto.custom_type,
                IsDistinct = dto.is_distinct,
                IsPublic = dto.is_public,
                MemberCount = dto.member_count,
                CreatedAt = dto.created_at,
                MyRole = MessageDtoMapper.ParseRole(dto.my_role)
                // TODO [GIM-9147-MessageCollection]: Parse last_message from API response
                // LastMessage = MessageDtoMapper.ToBusinessObject(dto.last_message)
            };
        }

        /// <summary>
        /// Convert ChannelBO to ChannelDTO
        /// </summary>
        public static ChannelDTO ToDto(ChannelBO bo)
        {
            if (bo == null)
            {
                return null;
            }

            return new ChannelDTO
            {
                channel_url = bo.ChannelUrl,
                name = bo.Name,
                cover_url = bo.CoverUrl,
                custom_type = bo.CustomType,
                is_distinct = bo.IsDistinct,
                is_public = bo.IsPublic,
                member_count = bo.MemberCount,
                created_at = bo.CreatedAt
                // TODO [GIM-9147-MessageCollection]: Map LastMessage back to DTO
                // last_message = MessageDtoMapper.ToDto(bo.LastMessage)
            };
        }
    }
}
