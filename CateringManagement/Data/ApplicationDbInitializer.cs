using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CateringManagement.Data
{
    public static class ApplicationDbInitializer
    {
        public static async void Seed(IApplicationBuilder applicationBuilder)
        {
            ApplicationDbContext context = applicationBuilder.ApplicationServices.CreateScope()
                .ServiceProvider.GetRequiredService<ApplicationDbContext>();
            try
            {
                //Create the database if it does not exist and apply the Migration
                //context.Database.EnsureDeleted();
                //context.Database.EnsureCreated();
                context.Database.Migrate();

                //Create Roles
                var RoleManager = applicationBuilder.ApplicationServices.CreateScope()
                    .ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                string[] roleNames = { "Admin","Security", "Supervisor", "Staff" };
                IdentityResult roleResult;
                foreach (var roleName in roleNames)
                {
                    var roleExist = await RoleManager.RoleExistsAsync(roleName);
                    if (!roleExist)
                    {
                        roleResult = await RoleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }
                //Create Users
                var userManager = applicationBuilder.ApplicationServices.CreateScope()
                    .ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                /*Create user admin@outlook.com with password "Pa55w@rd" and add it
                to both the Admin and Security roles*/
                if (userManager.FindByEmailAsync("admin@outlook.com").Result == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "admin@outlook.com",
                        Email = "admin@outlook.com",
                        EmailConfirmed = true
                    };

                    IdentityResult result = userManager.CreateAsync(user, "Pa55w@rd").Result;

                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(user, "Admin").Wait();
                        userManager.AddToRoleAsync(user, "Security").Wait();
                    }
                }
                /*Create user security@outlook.com with password "Pa55w@rd" and add it
                to just the Security role*/
                if (userManager.FindByEmailAsync("security@outlook.com").Result == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "security@outlook.com",
                        Email = "security@outlook.com",
                        EmailConfirmed = true
                    };

                    IdentityResult result = userManager.CreateAsync(user, "Pa55w@rd").Result;

                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(user, "Security").Wait();
                    }
                }
                /*Create user supervisor@outlook.com with password "Pa55w@rd" and add
                it to the Supervisor role. */
                if (userManager.FindByEmailAsync("supervisor@outlook.com").Result == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "supervisor@outlook.com",
                        Email = "supervisor@outlook.com",
                        EmailConfirmed = true
                    };

                    IdentityResult result = userManager.CreateAsync(user, "Pa55w@rd").Result;

                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(user, "Supervisor").Wait();
                    }
                }
                /*Create user staff@outlook.com with password "Pa55w@rd" and add it to
                the Staff role*/
                if (userManager.FindByEmailAsync("staff@outlook.com").Result == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "staff@outlook.com",
                        Email = "staff@outlook.com",
                        EmailConfirmed = true
                    };

                    IdentityResult result = userManager.CreateAsync(user, "Pa55w@rd").Result;

                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(user, "Staff").Wait();
                    }
                }
                /*Create user user@outlook.com with password "Pa55w@rd”, not in any role.*/
                if (userManager.FindByEmailAsync("user@outlook.com").Result == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "user@outlook.com",
                        Email = "user@outlook.com",
                        EmailConfirmed = true
                    };

                    IdentityResult result = userManager.CreateAsync(user, "Pa55w@rd").Result;
                    //Not in any role
                }
                /*Also create three additional users with password "Pa55w@rd”, one for email
                jkaluba@niagaracollege.ca*/
                if (userManager.FindByEmailAsync("jkaluba@niagaracollege.ca").Result == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "jkaluba@niagaracollege.ca",
                        Email = "jkaluba@niagaracollege.ca",
                        EmailConfirmed = true
                    };

                    IdentityResult result = userManager.CreateAsync(user, "Pa55w@rd").Result;

                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(user, "Admin").Wait();
                        userManager.AddToRoleAsync(user, "Security").Wait();
                    }
                }
                /*one for dstovell@niagaracollege.ca */
                if (userManager.FindByEmailAsync("dstovell@niagaracollege.ca").Result == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "dstovell@niagaracollege.ca",
                        Email = "dstovell@niagaracollege.ca",
                        EmailConfirmed = true
                    };

                    IdentityResult result = userManager.CreateAsync(user, "Pa55w@rd").Result;

                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(user, "Admin").Wait();
                        userManager.AddToRoleAsync(user, "Security").Wait();
                    }
                }
                /*and one for your own College email address.*/
                if (userManager.FindByEmailAsync("jjacoborodrigue1@ncstudents.niagaracollege.ca").Result == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "jjacoborodrigue1@ncstudents.niagaracollege.ca",
                        Email = "jjacoborodrigue1@ncstudents.niagaracollege.ca",
                        EmailConfirmed = true
                    };

                    IdentityResult result = userManager.CreateAsync(user, "Pa55w@rd").Result;

                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(user, "Admin").Wait();
                        userManager.AddToRoleAsync(user, "Security").Wait();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.GetBaseException().Message);
            }
        }
    }
}
