using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Common;
using InterceptorsTest;

namespace GrpcClient
{
    class Program
    {
static async Task Main(string[] args)
{
    var services = new ServiceCollection();
    services.AddScoped<CreateCorrelationIdInterceptor>();
    services.AddScoped<SetCorrelationIdInterceptor>();
    services.AddGrpcClient<Greeter.GreeterClient>(o =>
        {
            o.Address = new Uri("https://localhost:5001");
        })
        .AddInterceptor<CreateCorrelationIdInterceptor>()
        .AddInterceptor<SetCorrelationIdInterceptor>();

    using var scope = services.BuildServiceProvider().CreateScope();
    var client = scope.ServiceProvider.GetService<Greeter.GreeterClient>();
    await client.SayHelloAsync(new HelloRequest());
}
    }
}

