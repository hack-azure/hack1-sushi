using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stripe;
using System;

[assembly: FunctionsStartup(typeof(HackAzure1.Sushi.Api.Startup))]

namespace HackAzure1.Sushi.Api
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<CustomerService>();
            builder.Services.AddSingleton<PaymentMethodService>();
            builder.Services.AddSingleton<InvoiceItemService>();
            builder.Services.AddSingleton<InvoiceService>();

            var config = builder.Services.BuildServiceProvider().GetRequiredService<IConfiguration>();
            // TODO: local.settings.json / Azure Functions の構成で、Stripe のシークレットキーを環境変数にセットする必要あり。
            // Stripe のシークレットキーは、Stripe Dashboard の 開発 > API キーにあります。
            var secretKey = config.GetValue<string>("Stripe:SecretKey");
            if (string.IsNullOrEmpty(secretKey)) throw new ArgumentException();
            StripeConfiguration.ApiKey = secretKey;
        }
    }
}