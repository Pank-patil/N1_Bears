using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Unique.Models;

namespace Unique.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<WhitelistEntry> WhitelistEntries { get; set; }
    }
}
