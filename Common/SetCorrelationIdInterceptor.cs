using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Common
{
public class CreateCorrelationIdInterceptor : Interceptor
{
    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        Trace.CorrelationManager.ActivityId = Trace.CorrelationManager.ActivityId != Guid.Empty ? Trace.CorrelationManager.ActivityId : Guid.NewGuid();
        Console.WriteLine($"Creating id {Trace.CorrelationManager.ActivityId}");
        return base.AsyncUnaryCall(request, context, continuation);
    }
}

public class SetCorrelationIdInterceptor : Interceptor
{
    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        Console.WriteLine("Setting id");
        var headers = new Metadata
        {
            new Metadata.Entry("activity-id", Trace.CorrelationManager.ActivityId.ToString())
        };

        if (context.Options.Headers != null)
        {
            foreach (var header in context.Options.Headers)
            {
                context.Options.Headers.Add(header);
            }
        }

        var newOptions = context.Options.WithHeaders(headers);
        var newContext = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, newOptions);
        return base.AsyncUnaryCall(request, newContext, continuation);
    }

    public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var header = context.RequestHeaders.First(x => x.Key == "activity-id");
        Trace.CorrelationManager.ActivityId = Guid.Parse(header.Value);
        return base.UnaryServerHandler(request, context, continuation);
    }
}
}
