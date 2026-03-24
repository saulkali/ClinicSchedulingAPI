using ClinicScheduling.Api.Common.InjectionDependency;

var builder = WebApplication.CreateBuilder(args);

// injection dependency
builder.Services.AddClinicSchedulingDbContext(builder.Configuration);
builder.Services.AddJWTAuth(builder.Configuration);
builder.Services.AddJwtServices();
builder.Services.AddCorsInjection(builder.Configuration);
builder.Services.AddRepositories();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMapperProfiles(builder.Configuration);


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("AllowFrontend");
app.MapControllers();

app.Run();

/// <summary>
/// is necesary for unittesting
/// </summary>
public partial class Program
{
}
