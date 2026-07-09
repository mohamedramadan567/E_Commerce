using E_Commerce.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace E_Commerce.API.Attributes
{
    public class RedisCacheAttribute : ActionFilterAttribute
    {
        private readonly int _durationInSec;

        public RedisCacheAttribute(int durationInSec = 60)
        {
            _durationInSec = durationInSec;
        }
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Get Cache Service From Container
            var cacheService = context.HttpContext.RequestServices.GetRequiredService<ICacheService>();

            var cacheKey = CreateCacheKey(context.HttpContext.Request);

            var data = await cacheService.GetDataAsync(cacheKey);

            // If Data Exists In Cache => Get Data From Cache + Skip EndPoint
            if(!string.IsNullOrEmpty(data))
            {
                context.Result = new ContentResult()
                {
                    Content = data,
                    ContentType = "application/json",
                    StatusCode = StatusCodes.Status200OK
                };
                return;
            }
            // If Data Not Exists => Execute Endpoint + Store Result In Cache If Result Is 200OK + Data

            var executedContext = await next.Invoke();
            if (executedContext.Result is OkObjectResult { Value: not null } ok)
            {
                await cacheService.SetDataAsync(cacheKey, ok.Value, TimeSpan.FromSeconds(_durationInSec));
            }

        }
        private static string CreateCacheKey(HttpRequest request)
        {
            var Key = new StringBuilder();
            Key.Append(request.Path);
            if (request.Query.Any())
            {
                Key.Append('?');
                foreach (var (k, v) in request.Query.OrderBy(x => x.Key))
                {
                    Key.Append(k).Append("=").Append(v).Append('&');
                }
            }

            return Key.ToString();
        }
    }
}
