using Microsoft.EntityFrameworkCore;
using POE_Project.Data;
using Microsoft.AspNetCore.Identity;
using POE_Project.Models;

namespace POE_Project
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add DbContext
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("CMCS_DB"));

            // Add Identity services
            builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

            // Add controllers with views
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            // ✅ ADDED: Account route
            app.MapControllerRoute(
                name: "account",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            // Default route
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages();

            // Seed roles and demo users
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                string[] roles = { "Lecturer", "Coordinator", "Manager", "HR" };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                // Default demo users
                var users = new List<(string email, string role)>
                {
                    ("lecturer@poe.com", "Lecturer"),
                    ("coordinator@poe.com", "Coordinator"),
                    ("manager@poe.com", "Manager"),
                    ("hr@poe.com", "HR")
                };

                foreach (var userDef in users)
                {
                    var existing = await userManager.FindByEmailAsync(userDef.email);
                    if (existing == null)
                    {
                        var user = new ApplicationUser
                        {
                            UserName = userDef.email,
                            Email = userDef.email,
                            Department = userDef.role
                        };

                        await userManager.CreateAsync(user, "Password123!");
                        await userManager.AddToRoleAsync(user, userDef.role);
                    }
                }
            }

            await app.RunAsync();
        }
    }
}