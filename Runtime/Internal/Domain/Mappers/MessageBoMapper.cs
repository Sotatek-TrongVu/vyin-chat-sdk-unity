using Gamania.GIMChat.Internal.Domain.Models;

namespace Gamania.GIMChat.Internal.Domain.Mappers
{
    public static class MessageBoMapper
    {
        public static GimBaseMessage ToPublicModel(MessageBO bo)
        {
            if (bo == null)
                return null;

            return new GimBaseMessage
            {
                MessageId = bo.MessageId,
                Message = bo.Message,
                ChannelUrl = bo.ChannelUrl,
                CreatedAt = bo.CreatedAt,
                Done = bo.Done,
                CustomType = bo.CustomType,
                Data = bo.Data,
                ReqId = bo.ReqId,
                Sender = ToPublicSender(bo.Sender)
            };
        }

        public static GimSender ToPublicSender(SenderBO bo)
        {
            if (bo == null)
                return null;

            return new GimSender
            {
                UserId = bo.UserId,
                Nickname = bo.Nickname,
                ProfileUrl = bo.ProfileUrl,
                Role = ToPublicRole(bo.Role)
            };
        }

        internal static GimRole ToPublicRole(RoleBO role)
        {
            return role switch
            {
                RoleBO.Operator => GimRole.Operator,
                _ => GimRole.None
            };
        }

        public static MessageBO ToBusinessObject(GimBaseMessage model)
        {
            if (model == null)
                return null;

            return new MessageBO
            {
                MessageId = model.MessageId,
                Message = model.Message,
                ChannelUrl = model.ChannelUrl,
                CreatedAt = model.CreatedAt,
                Done = model.Done,
                CustomType = model.CustomType,
                Data = model.Data,
                ReqId = model.ReqId,
                Sender = ToSenderBO(model.Sender)
            };
        }

        public static SenderBO ToSenderBO(GimSender model)
        {
            if (model == null)
                return null;

            return new SenderBO
            {
                UserId = model.UserId,
                Nickname = model.Nickname,
                ProfileUrl = model.ProfileUrl,
                Role = ToRoleBO(model.Role)
            };
        }

        internal static RoleBO ToRoleBO(GimRole role)
        {
            return role switch
            {
                GimRole.Operator => RoleBO.Operator,
                _ => RoleBO.None
            };
        }
    }
}
