using Amazon.Lambda.APIGatewayEvents;
using Microsoft.AspNetCore.Http;

namespace orcazap.local.host
{
    public static class LambdaBridge
    {
        public static async Task<APIGatewayHttpApiV2ProxyRequest> BuildRequest(
            HttpContext ctx,
            string routeKey,
            Dictionary<string, string> pathParameters = null)
        {
            string body = null;
            if (ctx.Request.ContentLength > 0 || ctx.Request.Headers.ContainsKey("Transfer-Encoding"))
            {
                using var reader = new StreamReader(ctx.Request.Body);
                body = await reader.ReadToEndAsync();
            }

            var headers = ctx.Request.Headers.ToDictionary(h => h.Key, h => string.Join(",", h.Value));
            var query = ctx.Request.Query.ToDictionary(q => q.Key, q => string.Join(",", q.Value));

            return new APIGatewayHttpApiV2ProxyRequest
            {
                RouteKey = routeKey,
                RawPath = ctx.Request.Path,
                RawQueryString = ctx.Request.QueryString.HasValue ? ctx.Request.QueryString.Value.TrimStart('?') : string.Empty,
                Headers = headers,
                QueryStringParameters = query.Count > 0 ? query : null,
                PathParameters = pathParameters ?? new Dictionary<string, string>(),
                StageVariables = new Dictionary<string, string>(),
                Body = body,
                IsBase64Encoded = false,
                RequestContext = new APIGatewayHttpApiV2ProxyRequest.ProxyRequestContext
                {
                    Http = new APIGatewayHttpApiV2ProxyRequest.HttpDescription
                    {
                        Method = ctx.Request.Method,
                        Path = ctx.Request.Path,
                        SourceIp = ctx.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1"
                    }
                }
            };
        }

        public static async Task WriteResponse(HttpContext ctx, APIGatewayHttpApiV2ProxyResponse response)
        {
            ctx.Response.StatusCode = response.StatusCode;
            if (response.Headers != null)
            {
                foreach (var h in response.Headers)
                {
                    if (h.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                        ctx.Response.ContentType = h.Value;
                    else if (!h.Key.StartsWith("Access-Control", StringComparison.OrdinalIgnoreCase))
                        ctx.Response.Headers[h.Key] = h.Value;
                }
            }
            if (!string.IsNullOrEmpty(response.Body))
                await ctx.Response.WriteAsync(response.Body);
        }
    }
}
