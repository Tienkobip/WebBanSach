using Microsoft.AspNetCore.Identity;
using WebApp.Api.Entities;

namespace WebApp.Api.Utilities
{
    public class UserSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

            var email = "jackacevn@gmail.com";
            var newUser = User.CreateCustomer(
                email,
                fullName: "Nguyễn Việt Tiến",
                phoneNumber: "0896699703"
            );

            newUser.NormalizedUserName = email.ToUpper();
            newUser.NormalizedEmail = email.ToUpper();
            newUser.EmailConfirmed = true;
            newUser.PhoneNumberConfirmed = true;
            newUser.UpdateProfile("Nguyễn Việt Tiến", "Gia Lai", new DateTime(2005, 12, 08));

            var rawPassword = "12345";
            var result = await userManager.CreateAsync(newUser, rawPassword);

            if (result.Succeeded)
            {
                // Gán quyền Admin cho tài khoản mới tạo
                await userManager.AddToRoleAsync(newUser, "Admin");
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Không thể seed tài khoản {email}: {errors}");
            }

        }
    }
}
