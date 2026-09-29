using Amazon.Lambda.APIGatewayEvents;
using System.Text.Json.Serialization;

namespace core.Configuration
{
    [JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
    [JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
    public partial class HttpApiJsonSerializerContext : JsonSerializerContext
    {
    }
}
