using HNOne.API.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace HNOne.API.Installers
{
    public class AuthenticationInstaller : IInstaller
    {
        public void InstallerService(IServiceCollection services, IConfiguration configuration)
        {
            var jwtConfiguration = new JwtConfiguration();
            configuration.GetSection(nameof(JwtConfiguration)).Bind(jwtConfiguration);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(option =>
            {
                option.SaveToken = true;
                option.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtConfiguration.JwtIssuer,
                    ValidAudience = jwtConfiguration.JwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfiguration.JwtSecurityKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });
        }
    }
}
