using System.Reflection;
using CSharpFunctionalExtensions;
using DeliveryApp.Api;
using DeliveryApp.Api.Adapters.BackgroundJobs;
using DeliveryApp.Core.Application.UseCases.Commands.AssignOrder;
using DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;
using DeliveryApp.Core.Application.UseCases.Commands.MoveCouriers;
using DeliveryApp.Core.Application.UseCases.Queries.GetAllCouriers;
using DeliveryApp.Core.Application.UseCases.Queries.GetUncompletedOrders;
using DeliveryApp.Core.Domain.Services;
using DeliveryApp.Core.Ports;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using OpenApi.Filters;
using OpenApi.Formatters;
using OpenApi.OpenApi;
using Primitives;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// Health Checks
builder.Services.AddHealthChecks();

// Cors
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.AllowAnyOrigin(); // Не делайте так в проде!
        });
});

// Configuration
builder.Services.ConfigureOptions<SettingsSetup>();
// var connectionString = builder.Configuration["CONNECTION_STRING"];
 var connectionString = "Server=localhost;Port=5432;User Id=postgres;Password=postgres;Database=delivery;";

// Domain Services
builder.Services.AddScoped<IDispatchService, DispatchService>();

// БД, ORM 
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseNpgsql(connectionString,
            sqlOptions => { sqlOptions.MigrationsAssembly("DeliveryApp.Infrastructure"); });
        options.EnableSensitiveDataLogging();
    }
);

// UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Repositories
builder.Services.AddScoped<ICourierRepository, CourierRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// MediatR
builder.Services.AddMediatR(config => config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// Commands
builder.Services.AddTransient<IRequestHandler<CreateOrderCommand, UnitResult<Error>>, CreateOrderCommandHandler>();
builder.Services.AddTransient<IRequestHandler<MoveCouriersCommand, UnitResult<Error>>, MoveCouriersCommandHandler>();
builder.Services.AddTransient<IRequestHandler<AssignOrderCommand, UnitResult<Error>>, AssignOrderCommandHandler>();

// Queries
builder.Services.AddTransient<IRequestHandler<GetAllCouriersQuery, GetAllCouriersResponse>>(_ =>
    new GetAllCouriersQueryHandler(connectionString));

builder.Services.AddTransient<IRequestHandler<GetUncompletedOrdersQuery, GetUncompletedOrdersResponse>>(_ =>
    new GetUncompletedOrdersQueryHandler(connectionString));

// HTTP Handlers
builder.Services.AddControllers(options => { options.InputFormatters.Insert(0, new InputFormatterStream()); })
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        options.SerializerSettings.Converters.Add(new StringEnumConverter
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        });
    });

builder.Services.AddQuartz(configure =>
{
    var assignOrdersJobKey = new JobKey(nameof(AssignOrdersJob));
    var moveCouriersJobKey = new JobKey(nameof(MoveCouriersJob));
    configure
        .AddJob<AssignOrdersJob>(assignOrdersJobKey)
        .AddTrigger(
            trigger => trigger.ForJob(assignOrdersJobKey)
                .WithSimpleSchedule(
                    schedule => schedule.WithIntervalInSeconds(1)
                        .RepeatForever()))
        .AddJob<MoveCouriersJob>(moveCouriersJobKey)
        .AddTrigger(
            trigger => trigger.ForJob(moveCouriersJobKey)
                .WithSimpleSchedule(
                    schedule => schedule.WithIntervalInSeconds(2)
                        .RepeatForever()));
    configure.UseMicrosoftDependencyInjectionJobFactory();
});
builder.Services.AddQuartzHostedService();

// Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("1.0.0", new OpenApiInfo
    {
        Title = "Delivery Service",
        Description = "Отвечает за диспетчеризацию доставки"
    });
    options.CustomSchemaIds(type => type.FriendlyId(true));
    options.IncludeXmlComments(
        $"{AppContext.BaseDirectory}{Path.DirectorySeparatorChar}{Assembly.GetEntryAssembly()?.GetName().Name}.xml");
    options.DocumentFilter<BasePathFilter>("");
    options.OperationFilter<GeneratePathParamsValidationFilter>();
});
builder.Services.AddSwaggerGenNewtonsoftSupport();


var app = builder.Build();

// -----------------------------------
// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseHsts();

app.UseHealthChecks("/health");
app.UseRouting();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseSwagger(c => { c.RouteTemplate = "openapi/{documentName}/openapi.json"; })
    .UseSwaggerUI(options =>
    {
        options.RoutePrefix = "openapi";
        options.SwaggerEndpoint("/openapi/1.0.0/openapi.json", "Swagger Delivery Service");
        options.RoutePrefix = string.Empty;
        options.SwaggerEndpoint("/openapi-original.json", "Swagger Delivery Service");
    });

app.UseCors();
app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

// Apply Migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.Run();