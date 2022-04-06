var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

string? PrintLogger(HttpContext context) => $"loggerErrorIsEnabled: {context.Items["loggerErrorIsEnabled"]}";
app.MapGet("/print-logger", PrintLogger).AddFilter<ServiceAccessingRouteHandlerFilter>();

app.MapGet("/custom-error-message/{name}", (string requiredName) => "Hi ${requiredName}!")
.AddFilter((routeHandlerContext, next) => async (context) =>
    {
        if (context.HttpContext.Response.StatusCode == 400)
        {
            return Results.Problem("New response", statusCode: 400);
        }
        return await next(context);
    });

app.Run();

class ServiceAccessingRouteHandlerFilter : IRouteHandlerFilter
{
    private readonly ILogger _logger;

    public ServiceAccessingRouteHandlerFilter(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<ServiceAccessingRouteHandlerFilter>();
    }

    public async ValueTask<object?> InvokeAsync(RouteHandlerInvocationContext context, RouteHandlerFilterDelegate next)
    {
        context.HttpContext.Items["loggerErrorIsEnabled"] = _logger.IsEnabled(LogLevel.Error);
        return await next(context);
    }
}
