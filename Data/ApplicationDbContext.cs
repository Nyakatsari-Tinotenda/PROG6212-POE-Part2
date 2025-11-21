using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using POE_Project.Models;
    

    namespace POE_Project.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                : base(options) { }

            public DbSet<Lecturer> Lecturers { get; set; }
            public DbSet<Claim> Claims { get; set; }
            public DbSet<Document> Documents { get; set; }
        }
    }

