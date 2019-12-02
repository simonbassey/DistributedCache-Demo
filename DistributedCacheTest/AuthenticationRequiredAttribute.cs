using System;
using Microsoft.AspNetCore.Mvc.Filters;
using DistributedCacheTest.Services;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Mvc;
using System.Net;
namespace DistributedCacheTest
{
    public class AuthenticationRequiredAttribute : ActionFilterAttribute
    {
        private ICacheService _distributedCache;
        public AuthenticationRequiredAttribute(ICacheService cacheService)
        {
            _distributedCache = cacheService;
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {

        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                StringValues authTokenKeys;
                var authToken = context.HttpContext.Request.Headers.TryGetValue("authTokenKey", out authTokenKeys);
                if (!authToken || !authTokenKeys[0].Contains(":"))
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }
                var token = authTokenKeys[0].Split(':');
                var tokenKeyInCache = $"{token[0]}_authToken";
                var keyExist = _distributedCache.KeyExist(tokenKeyInCache).Result;
                if (!keyExist)
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }
                var keyValue = _distributedCache.GetEntry<string>(tokenKeyInCache).Result;
                if (!token[1].Equals(keyValue))
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }

            }
            catch (Exception)
            {
                context.Result = new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
