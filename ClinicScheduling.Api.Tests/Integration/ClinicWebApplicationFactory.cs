using ClinicScheduling.Api.Common.Database.Context;
using ClinicScheduling.Api.Common.Database.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace ClinicScheduling.Api.Tests.Integration;

public class ClinicWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    public Guid ExistingUserId { get; private set; }
    public Guid ExistingRoleId { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ClinicSchedulingDbContext>>();
            services.RemoveAll<ClinicSchedulingDbContext>();

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddDbContext<ClinicSchedulingDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            var serviceProvider = services.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ClinicSchedulingDbContext>();

            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();

            SeedData(dbContext);
        });
    }

    private void SeedData(ClinicSchedulingDbContext dbContext)
    {
        var doctorRole = new RoleEntity
        {
            Id = Guid.NewGuid(),
            Name = "Doctor",
            IsActive = true
        };

        var patientRole = new RoleEntity
        {
            Id = Guid.NewGuid(),
            Name = "Patient",
            IsActive = true
        };

        dbContext.Roles.AddRange(doctorRole, patientRole);
        dbContext.SaveChanges();

        // guardamos role para tests
        ExistingRoleId = doctorRole.Id;

        var activeUser = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = "doctor@clinic.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            RoleId = doctorRole.Id,
            IsActive = true
        };

        var inactiveUser = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = "inactive@clinic.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            RoleId = doctorRole.Id,
            IsActive = false
        };

        dbContext.Users.AddRange(activeUser, inactiveUser);
        dbContext.SaveChanges();

        // guardamos user para tests
        ExistingUserId = activeUser.Id;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection?.Dispose();
        }

        base.Dispose(disposing);
    }
}