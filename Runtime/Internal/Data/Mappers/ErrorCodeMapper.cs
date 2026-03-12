using System;

namespace Gamania.GIMChat.Internal.Data.Mappers
{
    internal static class ErrorCodeMapper
    {
        /// <summary>
        /// Maps an integer error code (from JSON body) to GimErrorCode.
        /// </summary>
        public static GimErrorCode FromApiCode(int code)
        {
            // Cast directly if defined in Enum
            if (Enum.IsDefined(typeof(GimErrorCode), code))
            {
                return (GimErrorCode)code;
            }

            // Fallback to Unknown if unrecognized
            return GimErrorCode.UnknownError;
        }

        /// <summary>
        /// Maps HTTP status code to GimErrorCode when detailed API error is missing.
        /// </summary>
        public static GimErrorCode FromHttpStatusFallback(int statusCode)
        {
            return statusCode switch
            {
                400 => GimErrorCode.ErrInvalidValue,       // Bad Request
                401 => GimErrorCode.ErrInvalidSession,     // Unauthorized
                403 => GimErrorCode.ErrForbidden,          // Forbidden
                404 => GimErrorCode.ErrNotFound,           // Not Found (generic)
                412 => GimErrorCode.ErrPreconditionFailed, // Precondition Failed
                429 => GimErrorCode.ErrServerBusy,         // Too Many Requests
                500 => GimErrorCode.ErrInternal,           // Internal Server Error
                503 => GimErrorCode.ErrOtherService,       // Service Unavailable
                504 => GimErrorCode.ErrHttpRequestTimeout, // Gateway Timeout
                _ => GimErrorCode.NetworkError             // Default fallback for other HTTP errors
            };
        }
    }
}
