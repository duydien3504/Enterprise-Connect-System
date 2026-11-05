using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Domain.Entities;
using EnterpriseConnect.IdentityService.Domain.Enums;
using EnterpriseConnect.IdentityService.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EnterpriseConnect.IdentityService.Infrastructure.Persistence;

public static class IdentityDbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<IdentityDbContext>>();

        try
        {
            await context.Database.MigrateAsync();

            // 1. Seed Department (Lưu trước để tránh vòng lặp circular dependency)
            var itDepartmentId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var itDepartment = await context.Departments.FirstOrDefaultAsync(d => d.Id == itDepartmentId);
            if (itDepartment == null)
            {
                itDepartment = new Department
                {
                    Id = itDepartmentId,
                    DepartmentName = "Information Technology",
                    CreatedAt = DateTimeOffset.UtcNow
                };
                await context.Departments.AddAsync(itDepartment);
                await context.SaveChangesAsync();
            }

            // 2. Seed Role Admin
            var adminRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Id == adminRoleId);
            if (adminRole == null)
            {
                adminRole = new Role
                {
                    Id = adminRoleId,
                    RoleName = "Admin",
                    Description = "Quản trị viên toàn hệ thống",
                    CreatedAt = DateTimeOffset.UtcNow
                };
                await context.Roles.AddAsync(adminRole);
                await context.SaveChangesAsync();
            }

            // 3. Seed Permissions
            var defaultPermissions = new List<(Guid Id, string Name)>
            {
                (Guid.Parse("33333333-1111-1111-1111-111111111111"), "users.view"),
                (Guid.Parse("33333333-2222-2222-2222-222222222222"), "users.create"),
                (Guid.Parse("33333333-3333-3333-3333-333333333333"), "users.update"),
                (Guid.Parse("33333333-4444-4444-4444-444444444444"), "roles.manage"),
                (Guid.Parse("33333333-5555-5555-5555-555555555555"), "documents.manage")
            };

            foreach (var (permId, permName) in defaultPermissions)
            {
                var perm = await context.Permissions.FirstOrDefaultAsync(p => p.Id == permId);
                if (perm == null)
                {
                    perm = new Permission
                    {
                        Id = permId,
                        PermissionName = permName,
                        CreatedAt = DateTimeOffset.UtcNow
                    };
                    await context.Permissions.AddAsync(perm);
                }

                var existsRolePerm = await context.RolePermissions
                    .AnyAsync(rp => rp.RoleId == adminRoleId && rp.PermissionId == permId);

                if (!existsRolePerm)
                {
                    await context.RolePermissions.AddAsync(new RolePermission
                    {
                        RoleId = adminRoleId,
                        PermissionId = permId,
                        CreatedAt = DateTimeOffset.UtcNow
                    });
                }
            }
            await context.SaveChangesAsync();

            // 4. Seed Seed Users List
            var usersToSeed = new[]
            {
                new
                {
                    Id = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
                    Email = "duydien3504@gmail.com",
                    FullName = "Nguyễn Duy Điền",
                    Phone = "+84901234567",
                    Position = "Senior Enterprise Architect & Administrator",
                    AvatarUrl = "https://ui-avatars.com/api/?name=Duy+Dien&background=0D8ABC&color=fff",
                    Password = "Abcd1234@"
                },
                new
                {
                    Id = Guid.Parse("13371337-1337-1337-1337-133713371337"),
                    Email = "duydien1337@gmail.com",
                    FullName = "Duy Điền Dev",
                    Phone = "+84901337133",
                    Position = "Software Engineer",
                    AvatarUrl = "https://ui-avatars.com/api/?name=Duy+Dien&background=10B981&color=fff",
                    Password = "Abcd1234@"
                },
                new
                {
                    Id = Guid.Parse("20202020-2020-2020-2020-202020202020"),
                    Email = "themcao20@gmail.com",
                    FullName = "Cao Thêm",
                    Phone = "+84902020202",
                    Position = "Technical Lead & Solutions Architect",
                    AvatarUrl = "https://ui-avatars.com/api/?name=Cao+Them&background=6366F1&color=fff",
                    Password = "Abcd1234@"
                }
            };

            var seededUsers = new List<User>();

            foreach (var item in usersToSeed)
            {
                var existingUser = await context.Users
                    .Include(u => u.UserRoles)
                    .FirstOrDefaultAsync(u => u.Email == item.Email);

                var passwordHash = passwordHasher.HashPassword(item.Password);

                if (existingUser == null)
                {
                    var user = new User
                    {
                        Id = item.Id,
                        DepartmentId = itDepartmentId,
                        FullName = item.FullName,
                        Email = item.Email,
                        Phone = item.Phone,
                        Position = item.Position,
                        PasswordHash = passwordHash,
                        AvatarUrl = item.AvatarUrl,
                        Status = UserStatus.Active,
                        EmailConfirmed = true,
                        CreatedAt = DateTimeOffset.UtcNow
                    };

                    await context.Users.AddAsync(user);
                    await context.UserRoles.AddAsync(new UserRole
                    {
                        UserId = user.Id,
                        RoleId = adminRoleId,
                        CreatedAt = DateTimeOffset.UtcNow
                    });

                    await context.SaveChangesAsync();
                    logger.LogInformation("Đã khởi tạo thành công tài khoản: {Email}", item.Email);
                    seededUsers.Add(user);
                }
                else
                {
                    existingUser.FullName = item.FullName;
                    existingUser.PasswordHash = passwordHash;
                    existingUser.Status = UserStatus.Active;
                    existingUser.EmailConfirmed = true;
                    existingUser.Phone = item.Phone;
                    existingUser.Position = item.Position;
                    existingUser.AvatarUrl = item.AvatarUrl;

                    if (!existingUser.UserRoles.Any(ur => ur.RoleId == adminRoleId))
                    {
                        existingUser.UserRoles.Add(new UserRole
                        {
                            UserId = existingUser.Id,
                            RoleId = adminRoleId,
                            CreatedAt = DateTimeOffset.UtcNow
                        });
                    }

                    await context.SaveChangesAsync();
                    logger.LogInformation("Đã cập nhật tài khoản: {Email}", item.Email);
                    seededUsers.Add(existingUser);
                }
            }

            // Assign IT department manager if not assigned
            if (itDepartment.ManagerId == null && seededUsers.Count > 0)
            {
                itDepartment.ManagerId = seededUsers[0].Id;
                context.Departments.Update(itDepartment);
                await context.SaveChangesAsync();
            }

            // 5. Seed Contact giữa duydien1337@gmail.com và themcao20@gmail.com
            var user1 = seededUsers.FirstOrDefault(u => u.Email == "duydien1337@gmail.com");
            var user2 = seededUsers.FirstOrDefault(u => u.Email == "themcao20@gmail.com");

            if (user1 != null && user2 != null)
            {
                var existingContact = await context.Contacts
                    .FirstOrDefaultAsync(c =>
                        (c.RequesterId == user1.Id && c.ReceiverId == user2.Id) ||
                        (c.RequesterId == user2.Id && c.ReceiverId == user1.Id));

                if (existingContact == null)
                {
                    await context.Contacts.AddAsync(new Contact
                    {
                        Id = Guid.NewGuid(),
                        RequesterId = user1.Id,
                        ReceiverId = user2.Id,
                        Status = ContactStatus.Accepted,
                        CreatedAt = DateTimeOffset.UtcNow
                    });
                    await context.SaveChangesAsync();
                    logger.LogInformation("Đã khởi tạo kết nối Contact (Accepted) giữa {Email1} và {Email2}", user1.Email, user2.Email);
                }
                else if (existingContact.Status != ContactStatus.Accepted)
                {
                    existingContact.Status = ContactStatus.Accepted;
                    await context.SaveChangesAsync();
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi xảy ra trong quá trình Seed Database");
            throw;
        }
    }
}
