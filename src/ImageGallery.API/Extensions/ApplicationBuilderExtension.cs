using ImageGallery.API.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ImageGallery.API.Extensions
{
    public static class ApplicationBuilderExtension
    {
        public static void InitializeGalleryDatabase(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var contextApp = serviceScope.ServiceProvider.GetRequiredService<GalleryContext>();
                contextApp.Database.Migrate();
                contextApp.EnsureSeedDataForContext();
            }
        }
    }
}
