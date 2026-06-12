using SportAPI.Data;
using SportAPI.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

// Register Swagger generation services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register CORS service to allow cross-origin requests from Angular frontend
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("X-Instance-Id"); // Expose custom instance headers
    });
});

// Register the DbContext via extension method
builder.Services.AddSportDatabase(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Enable CORS middleware (must be called before mapping endpoints)
app.UseCors();

// Enable Swagger UI middleware globally
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SportAPI v1");
    c.RoutePrefix = string.Empty; // Serves Swagger UI at the root
});

app.UseHttpsRedirection();

// Map our 20 Sport API endpoints
app.MapSportEndpoints();

app.Run();
