using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Portal.Data;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Portal.Services;

namespace Portal
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            var jwtSecret = builder.Configuration["Jwt:Secret"];
            if (string.IsNullOrEmpty(jwtSecret))
                throw new InvalidOperationException("Chave JWT não configurada em appsettings.json");

            var key = System.Text.Encoding.UTF8.GetBytes(jwtSecret);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };
                options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Cookies["JWT_Token"];
                        if (!string.IsNullOrEmpty(token)) context.Token = token;
                        return System.Threading.Tasks.Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        context.Response.Redirect("/Account/Login");
                        context.HandleResponse();
                        return System.Threading.Tasks.Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IAuditService, AuditService>();
            builder.Services.AddScoped<AccountService>();
            builder.Services.AddScoped<ClassService>();
            builder.Services.AddScoped<ChallengeService>();
            builder.Services.AddScoped<DashboardService>();
            builder.Services.AddScoped<ScenarioService>();
            builder.Services.AddScoped<SimulationHistoryService>();
            builder.Services.AddMvc();
            builder.Services.AddHttpClient<SimulationService>((sp, client) =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var baseUrl = config["MsCafinApiUrl"] ?? config["BACKEND_URL"] ?? "http://127.0.0.1:8000/";
                if (!baseUrl.EndsWith('/')) baseUrl += "/";
                client.BaseAddress = new Uri(baseUrl);
            });

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                       .UseSnakeCaseNamingConvention());

            var app = builder.Build();

            app.UseStaticFiles();

            // Reconhecer headers do reverse proxy (Nginx)
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            using (var scope = app.Services.CreateScope())
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                var maxRetries = 6;
                var retryDelay = TimeSpan.FromSeconds(5);

                for (int i = 0; i < maxRetries; i++)
                {
                    try
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        DbInitializer.Initialize(context);
                        logger.LogInformation("Base de dados inicializada e seed aplicado com sucesso.");
                        break;
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning($"Erro na inicialização da BD (tentativa {i + 1} de {maxRetries}). A base de dados pode ainda estar a arrancar. A aguardar {retryDelay.TotalSeconds} segundos...");
                        if (i == maxRetries - 1)
                        {
                            logger.LogError(ex, "Falha fatal na inicialização da BD após várias tentativas.");
                        }
                        System.Threading.Thread.Sleep(retryDelay);
                    }
                }
            }

            app.Run();
        }
    }
}
