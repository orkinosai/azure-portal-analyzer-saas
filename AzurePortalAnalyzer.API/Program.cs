using Azure.Communication.Email;
using Azure.Storage.Blobs;
using AzurePortalAnalyzer.Core.Data;
using AzurePortalAnalyzer.Core.Services;
using AzurePortalAnalyzer.Core.Services.Implementations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework - Use InMemory for demo, SQL Server for production
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("AzurePortalAnalyzer"));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Add Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options => 
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// Add Azure services with fallback for development
builder.Services.AddSingleton(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("AzureBlobStorage");
    if (string.IsNullOrEmpty(connectionString) || connectionString == "UseDevelopmentStorage=true")
    {
        // Use a mock or development storage
        return new BlobServiceClient("UseDevelopmentStorage=true");
    }
    return new BlobServiceClient(connectionString);
});

builder.Services.AddSingleton(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("AzureCommunicationServices");
    if (string.IsNullOrEmpty(connectionString) || connectionString.Contains("your-acs-resource"))
    {
        // For development, create a mock EmailClient or use a test connection string
        return new EmailClient("endpoint=https://test.communication.azure.com/;accesskey=test");
    }
    return new EmailClient(connectionString);
});

// Add application services
builder.Services.AddScoped<IAzureAnalysisService, AzureAnalysisService>();
builder.Services.AddScoped<IDocumentGenerationService, DocumentGenerationService>();
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAnalysisOrchestrationService, AnalysisOrchestrationService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("https://localhost:7002", "http://localhost:5002") // Blazor client URLs
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowBlazorClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (app.Environment.IsDevelopment())
    {
        // For InMemory database, ensure created is sufficient
        context.Database.EnsureCreated();
    }
    else
    {
        // For SQL Server, run migrations
        context.Database.Migrate();
    }
}

app.Run();
