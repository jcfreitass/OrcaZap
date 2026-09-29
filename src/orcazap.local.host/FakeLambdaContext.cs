using Amazon.Lambda.Core;

namespace orcazap.local.host
{
    public class FakeLambdaContext : ILambdaContext
    {
        public string AwsRequestId => Guid.NewGuid().ToString();
        public IClientContext ClientContext => null;
        public string FunctionName => "local-host";
        public string FunctionVersion => "$LATEST";
        public ICognitoIdentity Identity => null;
        public string InvokedFunctionArn => "arn:aws:lambda:local:000000000000:function:local-host";
        public ILambdaLogger Logger => new ConsoleLambdaLogger();
        public string LogGroupName => "/aws/lambda/local";
        public string LogStreamName => "local-stream";
        public int MemoryLimitInMB => 512;
        public TimeSpan RemainingTime => TimeSpan.FromMinutes(5);

        private class ConsoleLambdaLogger : ILambdaLogger
        {
            public void Log(string message) => Console.Write(message);
            public void LogLine(string message) => Console.WriteLine(message);
        }
    }
}
