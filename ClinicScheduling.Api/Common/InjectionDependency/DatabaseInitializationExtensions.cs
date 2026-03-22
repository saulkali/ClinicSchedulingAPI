using ClinicScheduling.Api.Common.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace ClinicScheduling.Api.Common.InjectionDependency;

/// <summary>
/// Expone utilidades para garantizar la creación automática de la base de datos al iniciar la aplicación.
/// </summary>
public static class DatabaseInitializationExtensions
{
    /// <summary>
    /// Crea automáticamente la base de datos y sus tablas cuando aún no existen.
    /// </summary>
    /// <param name="app">Aplicación web configurada.</param>
    /// <returns>La misma instancia de la aplicación para encadenar configuración.</returns>
    public static WebApplication EnsureClinicSchedulingDatabaseCreated(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ClinicSchedulingDbContext>();

        dbContext.Database.EnsureCreated();

        return app;
    }
}
