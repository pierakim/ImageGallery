using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using ImageGallery.IdentityServer.DbContexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Security.Claims;

namespace ImageGallery.IdentityServer.Extensions
{
    public static class ApplicationBuilderExtension
    {
        public static void InitializeIdentityDatabase(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                //PersistedGrant
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                //Identity Configuration Context
                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();

                if (!context.Clients.Any())
                {
                    foreach (var client in Config.GetClients())
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in Config.GetIdentityResources())
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {
                    foreach (var resource in Config.GetApiResources())
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
            }
        }

        public static void InitializeAspNetUserDatabase(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                //App Context
                var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.Migrate();
            }
        }

        public static void SeedUserDatabase(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var userMgr = serviceScope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                var Frank = userMgr.FindByNameAsync("Frank").Result;
                if (Frank == null)
                {
                    Frank = new IdentityUser
                    {
                        UserName = "Frank"
                    };
                    var result = userMgr.CreateAsync(Frank, "password").Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }

                    result = userMgr.AddClaimsAsync(Frank, new Claim[]{
                        new Claim("given_name", "Frank"),
                        new Claim("family_name", "Underwood"),
                        new Claim("address", "Main Road 1"),
                        new Claim("role", "FreeUser"),
                        new Claim("country", "nl"),
                        new Claim("subscriptionlevel", "FreeUser")
                    }).Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }
                    Console.WriteLine("Frank created");
                }
                else
                {
                    Console.WriteLine("Frank already exists");
                }

                var Claire = userMgr.FindByNameAsync("Claire").Result;
                if (Claire == null)
                {
                    Claire = new IdentityUser
                    {
                        UserName = "Claire"
                    };
                    var result = userMgr.CreateAsync(Claire, "password").Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }

                    result = userMgr.AddClaimsAsync(Claire, new Claim[]{
                        new Claim("given_name", "Claire"),
                        new Claim("family_name", "Underwood"),
                        new Claim("address", "Main Road 2"),
                        new Claim("role", "PayingUser"),
                        new Claim("country", "be"),
                        new Claim("subscriptionlevel", "PayingUser")
                    }).Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }
                    Console.WriteLine("Claire created");
                }
                else
                {
                    Console.WriteLine("Claire already exists");
                }
            }
        }
    }
}
