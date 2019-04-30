using System.Reflection;
using IdentityServer4.EntityFramework.DbContexts;
using ImageGallery.IdentityServer.DbContexts;
using ImageGallery.IdentityServer.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ImageGallery.IdentityServer
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            const string identityServerConnectionString = @"Data Source=(LocalDb)\MSSQLLocalDB;database=ImageGallery.IdentityServer.DB;trusted_connection=yes;";
            const string aspNetIdentityconnectionString = @"Data Source=(LocalDb)\MSSQLLocalDB;database=ImageGallery.AspIdentityProvider.DB;trusted_connection=yes;";
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            //AspNet Core Identity
            services.ConfigureAspNetIdentity(aspNetIdentityconnectionString, migrationsAssembly);
            //Identity Server
            services.ConfigureIdentityServer(identityServerConnectionString, migrationsAssembly);
            //MassTransit + RabbitMQ
            services.ConfigureMassTransitRabbitMQ();
        }

        public void Configure(IApplicationBuilder app, 
            Microsoft.AspNetCore.Hosting.IHostingEnvironment env, 
            ApplicationDbContext applicationDbContext,
            ConfigurationDbContext configurationDbContext)
        {
            //To initialise and seed database with the config from Config.cs
            //Check if botth configuration and application context exist
            var applicationDbContextExist = (applicationDbContext.Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator).Exists();
            var ConfigurationDbContextExist = (configurationDbContext.Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator).Exists();

            if (!applicationDbContextExist)
            {
                //Create schema + seed users
                app.InitializeAspNetUserDatabase();
                app.SeedUserDatabase();
            }

            if (!ConfigurationDbContextExist)
            {
                //create identity schema
                app.InitializeIdentityDatabase();
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseIdentityServer();
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }
}
