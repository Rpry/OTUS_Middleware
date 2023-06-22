using System;
using Microsoft.AspNetCore.Http;

namespace Middleware.Utilities
{
    public static class RequestHelper
    {
        /// <summary>
        /// GetGuidFromHeader
        /// </summary>
        /// <param name="request"></param>
        /// <param name="nameToSearch"></param>
        /// <returns></returns>
        public static Guid GetGuidFromHeader(HttpRequest request, string nameToSearch)
        {
            var stringFromHeader = GetStringFromHeader(request, nameToSearch);
            if (string.IsNullOrEmpty(stringFromHeader))
            {
                return Guid.Empty;
            }

            if (Guid.TryParse(stringFromHeader, out var result))
            {
                return result;
            }
            return Guid.Empty;
        }
        
        /// <summary>
        /// GetStringFromHeader
        /// </summary>
        /// <param name="request"></param>
        /// <param name="nameToSearch"></param>
        /// <returns></returns>
        public static string GetStringFromHeader(HttpRequest request, string nameToSearch)
        {
            var containsKey = request?.Headers?.ContainsKey(nameToSearch);
            if (containsKey != null && containsKey.Value)
            {
                return request.Headers[nameToSearch];
            }
            return null;
        }
    }
}