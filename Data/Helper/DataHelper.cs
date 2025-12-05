using Microsoft.EntityFrameworkCore;

namespace KlodTattooWeb.Data.Helper
{
    public static class DataHelper
    {
        public static async Task ManageDataAsync(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                // Apply any pending migrations
                await dbContext.Database.MigrateAsync();
               
            }
        }
    }
}
