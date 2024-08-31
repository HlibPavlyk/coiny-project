using CoinyProject.Core.Domain.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Infrastructure.Data
{
    public static class SeedingExtensions
    {
        public static async Task DBEnsureCreated(this IApplicationBuilder applicationBuilder)
        {
            using (var scope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
                var db = dbContext.Database;

                await db.EnsureDeletedAsync();
                await db.EnsureCreatedAsync();

                if (!dbContext.DiscussionTopics.Any())
                {
                    await dbContext.DiscussionTopics.AddAsync(new DiscussionTopic { Name = "Soviet" });
                    await dbContext.DiscussionTopics.AddAsync(new DiscussionTopic { Name = "Tsar" });
                    await dbContext.SaveChangesAsync();
                }


            }
        }

   
    }
}
