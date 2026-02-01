using System.Collections.Concurrent;

namespace SmartSheetLicenceApi.Middleware
{
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly int _requestLimit;
        private static readonly ConcurrentDictionary<string, RequestInfo> _requests = new();
        public RateLimitMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _requestLimit = configuration.GetValue<int>("ApiSettings:RateLimitPerMinute", 60);
        }
        public async Task InvokeAsync(HttpContext context)
        {
            string? ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            DateTime currentTime = DateTime.UtcNow;
            if (!_requests.TryGetValue(ipAddress, out var requestInfo))
            {
                requestInfo = new RequestInfo { Count = 1, FirstRequestTime = currentTime };
                _requests[ipAddress] = requestInfo;
            }
            else
            {
                double timeDiff = (currentTime - requestInfo.FirstRequestTime).TotalMinutes;
                if (timeDiff < 1)
                {
                    if (requestInfo.Count >= _requestLimit)
                    {
                        context.Response.StatusCode = 429;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            success = false,
                            message = "Rate limit exceeded. Please try again later."
                        });
                        return;
                    }
                    requestInfo.Count++;
                }
                else
                {
                    requestInfo.Count = 1;
                    requestInfo.FirstRequestTime = currentTime;
                }
            }
            await _next(context);
        }
        private class RequestInfo
        {
            public int Count { get; set; }
            public DateTime FirstRequestTime { get; set; }
        }
    }
}