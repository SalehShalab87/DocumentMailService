using DocumentMailService.Models;
using DocumentMailService.Services;
using DocumentAutomation.Library.Services;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure email settings
var emailConfig = new EmailConfiguration();
builder.Configuration.GetSection("EmailSettings").Bind(emailConfig);
builder.Services.AddSingleton(emailConfig);

// Register Document Automation services from library
builder.Services.AddSingleton<ITemplateService, TemplateService>();
builder.Services.AddScoped<IDocumentGenerationService, DocumentGenerationService>();
builder.Services.AddScoped<IDocumentEmbeddingService, DocumentEmbeddingService>();

// Register Email service
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();