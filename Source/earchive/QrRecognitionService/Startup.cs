using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Earchive.QrService
{
    public sealed class Startup
    {
        private const int MaxMessageSize = 32 * 1024 * 1024;

        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc(options =>
            {
                options.MaxReceiveMessageSize = MaxMessageSize;
                options.MaxSendMessageSize = MaxMessageSize;
                options.EnableDetailedErrors = true;
            });

            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = MaxMessageSize;
            });

            services.AddSingleton(serviceProvider =>
            {
                var modelsPath = _configuration["Qr:ModelsPath"];

                if (string.IsNullOrWhiteSpace(modelsPath))
                    modelsPath = Path.Combine(AppContext.BaseDirectory, "Models");
                else if (!Path.IsPathRooted(modelsPath))
                    modelsPath = Path.Combine(AppContext.BaseDirectory, modelsPath);

                var logger = serviceProvider.GetRequiredService<ILogger<WeChatQrRecognizer>>();
                return new WeChatQrRecognizer(modelsPath, logger);
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            // Клиент приложения общается по gRPC-Web (HTTP/1.1), чтобы работать
            // из .NET Framework 4.6.1 без HTTP/2.
            app.UseGrpcWeb();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<QrRecognitionGrpcService>().EnableGrpcWeb();
                endpoints.MapGet("/", context =>
                    context.Response.WriteAsync("Earchive QR recognition service is running."));
            });
        }
    }
}
