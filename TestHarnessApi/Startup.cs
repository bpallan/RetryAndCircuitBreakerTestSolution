using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Swashbuckle.AspNetCore.Swagger;
using TestHarnessApi.ExternalProxy;

namespace TestHarnessApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            
            services.AddHttpClient<IExternalApiClient, ExternalApiClient>()
                .AddPolicyHandler(GetSimpleRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());

            services.AddSwaggerGen(context =>
            {
                context.SwaggerDoc("v1", new Info() { Title = "Polly Example", Version = "v1" });
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        private IAsyncPolicy<HttpResponseMessage> GetSimpleRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .RetryAsync(retryCount: 1);            
        }

        private IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(2, TimeSpan.FromSeconds(30));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();

            app.UseSwaggerUI(context => { context.SwaggerEndpoint("/swagger/v1/swagger.json", "Polly Example V1"); });

            app.UseMvc();
        }
    }
}
