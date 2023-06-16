using Microsoft.AspNetCore.Mvc.Versioning;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApiVersioning(
    options =>
    {
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    });
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();
app.Run();

#pragma warning disable S1118 // This class is needed for API tests
public partial class Program {}
#pragma warning restore S1118
