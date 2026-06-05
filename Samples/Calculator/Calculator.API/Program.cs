using Asp.Versioning;

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
