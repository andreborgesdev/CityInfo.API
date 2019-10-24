using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using CityInfo.API.Services;
using Microsoft.Extensions.Configuration;
using CityInfo.API.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API
{
    public class Startup
    {
        public static IConfigurationRoot Configuration;

        public Startup(IHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Auto Mapper Configurations
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });

            IMapper mapper = mappingConfig.CreateMapper();
            services.AddSingleton(mapper);

            services.AddMvc(option => option.EnableEndpointRouting = false)
                .AddMvcOptions(o => o.OutputFormatters.Add(
                    new XmlDataContractSerializerOutputFormatter()))
                .AddNewtonsoftJson();
            //.AddNewtonsoftJson(o =>
            //{
            //    if (o.SerializerSettings.ContractResolver != null)
            //    {
            //        var castedResolver = o.SerializerSettings.ContractResolver
            //            as DefaultContractResolver;
            //        castedResolver.NamingStrategy = null;
            //    }
            //});


#if DEBUG
            services.AddTransient<IMailService, LocalMailService>();
#else
            services.AddTransient<IMailService, CloudMailService>();
#endif

            var connectionString = Startup.Configuration["ConnectionStrings:CityInfoDBConnectionString"];
            services.AddDbContext<CityInfoContext>(o => o.UseSqlServer(connectionString));

            services.AddScoped<ICityInfoRepository, CityInfoRepository>();

            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
            });

            services.AddVersionedApiExplorer(options => options.SubstituteApiVersionInUrl = true);

            services.AddOpenApiDocument(config =>
            {
                
                config.FlattenInheritanceHierarchy = true;
                config.PostProcess = document =>
                {
                    document.Info.Version = "1.0";
                    document.Info.Title = "CityAPI";
                    document.Info.Description = "API to manage cities";
                    document.Info.TermsOfService = new Uri("https://pt.primaverabss.com/pt/site/termos-de-utilizacao/").ToString();
                    document.Info.Contact = new NSwag.OpenApiContact
                    {
                        Name = "PRIMAVERA BSS",
                        Email = string.Empty,
                        Url = new Uri("https://pt.primaverabss.com/pt/").ToString()
                    };
                    document.Info.License = new NSwag.OpenApiLicense
                    {
                        Name = "Copyright",
                        Url = new Uri("https://pt.primaverabss.com/pt/").ToString()
                    };
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            CityInfoContext cityInfoContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            cityInfoContext.EnsureSeedDataForContext();

            app.UseStatusCodePages();

            app.UseMvc();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });

            app.UseOpenApi();
            app.UseSwaggerUi3();
        }
    }
}
