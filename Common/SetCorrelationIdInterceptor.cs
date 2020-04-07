using System;
using System.Diagnostics;
using System.Linq;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Common
{
    public class SetCorrelationIdInterceptor : Interceptor
    {
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            // HERE THE EXCEPTION IS THROWN
            var entry = new Metadata.Entry("activity-id", Trace.CorrelationManager.ActivityId.ToString());
            context.Options.Headers.Add(entry);
            return base.AsyncUnaryCall(request, context, continuation);
        }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context, AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            var header = context.Options.Headers.FirstOrDefault(x=>x.Key == "activity-id");
            Trace.CorrelationManager.ActivityId = header != null ? Guid.Parse(header.Value) : Guid.Empty;
            return base.AsyncServerStreamingCall(request, context, continuation);
        }
    }
}
