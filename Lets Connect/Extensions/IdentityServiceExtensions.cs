using Lets_Connect.Data;
using Lets_Connect.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Lets_Connect.Extensions
{
    public static class IdentityServiceExtensions
    {
        public static IServiceCollection AddIDentityServices(this IServiceCollection services,
                                                             IConfiguration configuration)
        {
            services.AddIdentityCore<User>(opt =>
            {
                opt.Password.RequireNonAlphanumeric = false;
            }).AddRoles<Roles>()
            .AddRoleManager<RoleManager<Roles>>()
            .AddEntityFrameworkStores<DataContext>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).
                AddJwtBearer(options =>
                {
                    var tokenKey = configuration["TokenKey"] ?? throw new Exception("Token Key not found");
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;
                            if(!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorizationBuilder().AddPolicy("RequiredAdminRole", policy => policy.RequireRole("Admin"))
                                              .AddPolicy("ModeratePhotoRole", policy => policy.RequireRole("Admin", "Moderator"));
            return services;
        }
    }
}
