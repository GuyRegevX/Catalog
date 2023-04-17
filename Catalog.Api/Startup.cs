using System.Net.Mime;
using System.Text.Json;
using Catalog.Api.Repositories;
using Catalog.Repositories;
using Catalog.Setting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Catalog.Api;

public class Startup
{
    private const string ServiceName = "Catalog";

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
        BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));

        var mongoDbSettings = Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();

        services.AddSingleton<IMongoClient>(ServiceProvider =>
        {
            return new MongoClient(mongoDbSettings.ConnectionString);
        });
        services.AddSingleton<IItemRepository, MongoDbItemsRepository>();
        services.AddControllers(options =>
        {
            options.SuppressAsyncSuffixInActionNames = false;
        });
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo{Title = "Catalog", Version = "a" });
        });
        services.AddHealthChecks()
                .AddMongoDb(mongoDbSettings.ConnectionString,
                           name: "mongodb",
                           timeout:TimeSpan.FromSeconds(3),
                           tags:new[]{"ready"}
                           );
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c=>c.SwaggerEndpoint("/swagger/v1/swagger.json", Startup.ServiceName));
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();
        app.UseEndpoints(endpoint =>
        {
            endpoint.MapControllers();
            endpoint.MapHealthChecks("/health/ready", new HealthCheckOptions()
            {
                Predicate = (check) => check.Tags.Contains("ready"),
                ResponseWriter = async(context, report) =>
                {
                    var result = JsonSerializer.Serialize(
                        new
                        {
                            status= report.Status,
                            checks = report.Entries.Select(entry=>new
                            {
                                name = entry.Key,
                                status = entry.Value.Status.ToString(),
                                exception = entry.Value.Exception != null ? entry.Value.Exception.Message :null,
                                duration = entry.Value.Duration.ToString()
                            })
                        }
                    );
                    context.Response.ContentType = MediaTypeNames.Application.Json;
                    await context.Response.WriteAsync(result);
                }
            });
            endpoint.MapHealthChecks("/health/live", new HealthCheckOptions()
            {
                Predicate = (_) => false
            });
        });
        
    }

}