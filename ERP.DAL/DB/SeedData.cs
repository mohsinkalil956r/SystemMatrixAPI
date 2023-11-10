using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ZATCA.DAL.DB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace ZATCA.DAL.DB
{
    public class SeedData
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            SeedUsers(modelBuilder);
            SeedRoles(modelBuilder);
            SeedPermissions(modelBuilder);
            SeedUserRoles(modelBuilder);
            SeedUsZATCAermissions(modelBuilder); 
        }

        private static void SeedUsers(ModelBuilder builder)
        {
            var userList = new List<SystemUser> 
            {
                new SystemUser()
                {
                    Id = 1,
                    FirstName = "Test",
                    LastName = "Admin",
                    UserName = "admin@gmail.com",
                    NormalizedUserName = "ADMIN@GMAIL.COM",
                    Email = "admin@gmail.com",
                    NormalizedEmail = "ADMIN@GMAIL.COM",
                    LockoutEnabled = false,
                    EmailConfirmed = true,
                    PhoneNumber = "1234567890",
                },
                new SystemUser()
                {
                    Id = 2,
                    FirstName = "System",
                    LastName = "User",
                    UserName = "system.user@gmail.com",
                    NormalizedUserName = "SYSTEM.USER@GMAIL.COM",
                    Email = "system.user@gmail.com",
                    NormalizedEmail = "SYSTEM.USER@GMAIL.COM",
                    LockoutEnabled = false,
                    EmailConfirmed = true,
                    PhoneNumber = "1234567890",
                }
            };

            var passwordHasher = new PasswordHasher<SystemUser>();

            userList.ForEach(user =>
            {
                user.PasswordHash = passwordHasher.HashPassword(user, "Admin*123");
            });

            builder.Entity<SystemUser>().HasData(userList);
        }

        private static void SeedPermissions(ModelBuilder builder)
        {
            var permissionsData = new List<Permission>();
            foreach(int e in Enum.GetValues(typeof(PERMISSIONS)))
            {
                permissionsData.Add(new Permission
                {
                    Id = e,
                    Name = ((PERMISSIONS)e).ToString(),
                    Description = ((PERMISSIONS)e).ToString(),
                });
            }
            builder.Entity<Permission>().HasData(permissionsData);
        }

        private static void SeedRoles(ModelBuilder builder)
        {
            var rolesData = new List<Role>();

            foreach (int e in Enum.GetValues(typeof(ROLES)))
            {
                rolesData.Add(new Role
                {
                    Id = e,
                    Name = ((ROLES)e).ToString(),
                    NormalizedName = ((ROLES)e).ToString().ToUpper(),
                    ConcurrencyStamp = ""
                });
            }

            builder.Entity<Role>().HasData(rolesData);
        }

        private static void SeedUserRoles(ModelBuilder builder)
        {
            builder.Entity<UserRole>().HasData(
                new UserRole() { RoleId = (int)ROLES.Admin, UserId = 1 },
                new UserRole() { RoleId = (int)ROLES.Company, UserId = 2 }
                );
        }

        private static void SeedUsZATCAermissions(ModelBuilder builder)
        {
            var usZATCAermissionsData = new List<UsZATCAermission>();
            foreach (int e in Enum.GetValues(typeof(PERMISSIONS)))
            {
                usZATCAermissionsData.Add(new UsZATCAermission
                {
                    PermissionId = e,
                    UserId = 2,
                });
            }

            builder.Entity<UsZATCAermission>().HasData(usZATCAermissionsData);
        }
    }
}
