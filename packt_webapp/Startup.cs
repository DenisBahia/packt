using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.Web;
using packt_webapp.Dtos;
using packt_webapp.Entities;
using packt_webapp.Middlewares;
using packt_webapp.Repositories;
using Swashbuckle.AspNetCore.Swagger;

namespace packt_webapp
{
    public class Startup
    {

        public IConfigurationRoot configuration { get; }

        public Startup(IHostingEnvironment env)
        {

            var builder = new ConfigurationBuilder()
                   .SetBasePath(env.ContentRootPath)
                   .AddJsonFile("appsettings.json");

            env.ConfigureNLog("nlog.config");

            configuration = builder.Build();

            Console.Write($" ----> From Config: {configuration["Name"]}");

        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            services.AddDbContext<PacktDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<ICustomerRepository, CustomerRepository>();

            services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc("v1", new Info { Title= "My first WebAPI", Version="v1" });
            });

            services.AddMvc(configuration =>
            {
                configuration.ReturnHttpNotAcceptable = true;
                configuration.OutputFormatters.Add(new XmlSerializerOutputFormatter());
                configuration.InputFormatters.Add(new XmlSerializerInputFormatter());
            });

            services.AddApiVersioning(config =>
            {
                config.ReportApiVersions = true;
                config.AssumeDefaultVersionWhenUnspecified = true;
                config.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {

            loggerFactory.AddConsole(configuration.GetSection("Logging"));
            loggerFactory.AddDebug(LogLevel.Error);
            

            loggerFactory.AddNLog();
            app.AddNLogWeb();


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(errorApp => {
                    errorApp.Run(async context => {
                        context.Response.StatusCode = 500;
                        context.Response.ContentType = "text/plain";
                        var errorFeature = context.Features.Get<IExceptionHandlerFeature> ();
                        if (errorFeature != null)
                        {
                            var logger = loggerFactory.CreateLogger("Global exception logger");
                            logger.LogError(500, errorFeature.Error, errorFeature.Error.Message);
                        }
                        await context.Response.WriteAsync("There is an error.");
                    });
                });
            }

            app.UseApiVersioning();
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseSwagger();

            app.UseSwaggerUI(config =>
            {
                config.SwaggerEndpoint("/swagger/v1/swagger.json", "My first WebAPI");
            });

            //app.UseMiddleware<CustomMiddleware>();

            AutoMapper.Mapper.Initialize(mapper =>
            {
                mapper.CreateMap<Customer, CustomerDto>().ReverseMap();
                mapper.CreateMap<Customer, CustomerUpdateDto>().ReverseMap();
            });

            app.UseMvc();

        }
    }
}
