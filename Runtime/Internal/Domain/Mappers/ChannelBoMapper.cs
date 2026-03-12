using Gamania.GIMChat.Internal.Domain.Models;

namespace Gamania.GIMChat.Internal.Domain.Mappers
{
    /// <summary>
    /// Mapper for converting between ChannelBO and GimGroupChannel (Public API Model)
    /// Domain Layer responsibility: Business Object ↔ Public Model conversion
    /// Used by UseCases to convert internal BO to public-facing models
    /// </summary>
    public static class ChannelBoMapper
    {
        /// <summary>
        /// Convert ChannelBO to GimGroupChannel (Public API Model)
        /// </summary>
        public static GimGroupChannel ToPublicModel(ChannelBO bo)
        {
            if (bo == null)
            {
                return null;
            }

            return new GimGroupChannel
            {
                ChannelUrl = bo.ChannelUrl,
                Name = bo.Name,
                CoverUrl = bo.CoverUrl,
                CustomType = bo.CustomType,
                IsDistinct = bo.IsDistinct,
                IsPublic = bo.IsPublic,
                MemberCount = bo.MemberCount,
                CreatedAt = bo.CreatedAt,
                MyRole = MessageBoMapper.ToPublicRole(bo.MyRole)
                // TODO [GIM-9147-MessageCollection]: Map LastMessage
                // LastMessage = MessageBoMapper.ToPublicModel(bo.LastMessage)
            };
        }

        /// <summary>
        /// Convert GimGroupChannel to ChannelBO
        /// Used for input parameters (e.g., update operations)
        /// </summary>
        public static ChannelBO ToBusinessObject(GimGroupChannel model)
        {
            if (model == null)
            {
                return null;
            }

            return new ChannelBO
            {
                ChannelUrl = model.ChannelUrl,
                Name = model.Name,
                CoverUrl = model.CoverUrl,
                CustomType = model.CustomType,
                IsDistinct = model.IsDistinct,
                IsPublic = model.IsPublic,
                MemberCount = model.MemberCount,
                CreatedAt = model.CreatedAt,
                MyRole = MessageBoMapper.ToRoleBO(model.MyRole)
                // TODO [GIM-9147-MessageCollection]: Map LastMessage
                // LastMessage = MessageBoMapper.ToBusinessObject(model.LastMessage)
            };
        }
    }
}
