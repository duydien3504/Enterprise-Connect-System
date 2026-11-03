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

            // 4. Seed User "duydien3504@gmail.com"
            var userEmail = "duydien3504@gmail.com";
            var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (existingUser == null)
            {
                var userId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
                var user = new User
                {
                    Id = userId,
                    DepartmentId = itDepartmentId,
                    FullName = "Nguyễn Duy Điền",
                    Email = userEmail,
                    Phone = "+84901234567",
                    Position = "Senior Enterprise Architect & Administrator",
                    PasswordHash = passwordHasher.HashPassword("Abcd1234@"),
                    AvatarUrl = "https://ui-avatars.com/api/?name=Duy+Dien&background=0D8ABC&color=fff",
                    Status = UserStatus.Active,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();

                // Assign Admin Role to User
                await context.UserRoles.AddAsync(new UserRole
                {
                    UserId = userId,
                    RoleId = adminRoleId,
                    CreatedAt = DateTimeOffset.UtcNow
                });

                // Update manager for IT department
                itDepartment.ManagerId = userId;
                context.Departments.Update(itDepartment);

                await context.SaveChangesAsync();
                logger.LogInformation("Đã khởi tạo thành công tài khoản: {Email}", userEmail);
            }
            else
            {
                existingUser.PasswordHash = passwordHasher.HashPassword("Abcd1234@");
                existingUser.Status = UserStatus.Active;
                await context.SaveChangesAsync();
                logger.LogInformation("Đã cập nhật mật khẩu cho tài khoản: {Email}", userEmail);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi xảy ra trong quá trình Seed Database");
            throw;
        }
    }
}
