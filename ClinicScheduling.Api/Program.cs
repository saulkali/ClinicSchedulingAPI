using ClinicScheduling.Api.Common.InjectionDependency;

var builder = WebApplication.CreateBuilder(args);

// injection dependency
builder.Services.AddClinicSchedulingDbContext(builder.Configuration);
builder.Services.AddJWTAuth(builder.Configuration);
builder.Services.AddJwtServices();
builder.Services.AddRepositories();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

/// <summary>
/// is necesary for unittesting
/// </summary>
public partial class Program
{
}