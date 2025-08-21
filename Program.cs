using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using UltraStrore.Controllers;
using UltraStrore.Data;
using UltraStrore.Helper;
using UltraStrore.Hubs;
using UltraStrore.Middleware;
using UltraStrore.Repository;
using UltraStrore.Services;
using UltraStrore.Utils;

namespace UltraStrore
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            if (builder.Environment.IsDevelopment())
            {

            }
            else
            {
                // Azure deployment - lắng nghe trên port 8080
                builder.WebHost.UseUrls("http://*:8080");
            }

            // Thêm logging chi tiết hơn
            builder.Logging.AddConsole();
            builder.Logging.SetMinimumLevel(LogLevel.Information);

            // Basic services
            builder.Services.AddControllers();
            builder.Services.AddHttpClient();
            builder.Services.AddHttpContextAccessor();

            // Database configuration
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // HttpClient services
            builder.Services.AddScoped<IGHNService, GHNService>();
            builder.Services.AddHttpClient<IGHNService, GHNService>(client =>
            {
                client.BaseAddress = new Uri("https://dev-online-gateway.ghn.vn/shiip/public-api/");
                client.DefaultRequestHeaders.Add("User-Agent", "UltraStrore-App/1.0");
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            builder.Services.AddHttpClient<IGoogleApisServices, GoogleApisServices>();

            // Scoped services
            builder.Services.AddScoped<IOrderNotificationService, OrderNotificationService>();
            builder.Services.AddScoped<IQRCodeService, QRCodeService>();
            builder.Services.AddScoped<IGeminiServices, GeminiServices>();
            builder.Services.AddScoped<ICartServices, CartServices>();
            builder.Services.AddScoped<ISanPhamServices, SanPhamServices>();
            builder.Services.AddScoped<INguoiDungServices, NguoiDungServices>();
            builder.Services.AddScoped<IDanhSachDiaChiServices, DanhSachDiaChiServices>();
            builder.Services.AddScoped<ICommetServices, CommetServices>();
            builder.Services.AddScoped<IVoucherServices, VoucherServices>();
            builder.Services.AddScoped<IJwtTokenServices, JwtTokenGenerator>();
            builder.Services.AddScoped<IEmailServices, EmailServices>();
            builder.Services.AddScoped<ILienHeServices, LienHeServices>();
            builder.Services.AddScoped<ITinNhanServices, TinNhanServices>();
            builder.Services.AddScoped<IThongKeServices, ThongKeServices>();
            builder.Services.AddScoped<IKhuyenMaiServices, KhuyenMaiServices>();
            builder.Services.AddScoped<IBlogServices, BlogServices>();
            builder.Services.AddScoped<IYeuThichServices, YeuThichServices>();
            builder.Services.AddScoped<IComboServices, ComboServices>();
            builder.Services.AddScoped<IGiaoDienServices, GiaoDienServices>();
            builder.Services.AddScoped<ILoaiSanPhamServices, LoaiSanPhamServices>();
            builder.Services.AddScoped<IThuongHieuServices, ThuongHieuServices>();
            builder.Services.AddScoped<IKichThuocServices, KichThuocServices>();
            builder.Services.AddScoped<ICheckOutServices, CheckOutService>();
            builder.Services.AddScoped<IVnPayServies, VnPayService>();
            builder.Services.AddScoped<IHashTagServices, HashTagServices>();
            builder.Services.AddScoped<IOpenAIServices, OpenAIServices>();
            builder.Services.AddScoped<ITelegramServices, TelegramServices>();
            // Singleton services
            builder.Services.AddSingleton<ITokenBlacklistService, TokenBlacklistService>();
            builder.Services.AddTransient<EmailService>();

            // Configuration bindings
            builder.Services.Configure<VnPayConfig>(builder.Configuration.GetSection("VnPay"));
            builder.Services.Configure<OpenAISettings>(builder.Configuration.GetSection("OpenAI"));
            builder.Services.Configure<GoogleApisSettings>(builder.Configuration.GetSection("GoogleApis"));
            builder.Services.Configure<GeminiSettings>(builder.Configuration.GetSection("Authentication"));

            builder.Services.AddSingleton(sp =>
                sp.GetRequiredService<IOptions<VnPayConfig>>().Value);
            builder.Services.AddSingleton(resolver =>
                resolver.GetRequiredService<IOptions<GeminiSettings>>().Value);

            // SignalR
            builder.Services.AddSignalR();

            // Cache and Session
            builder.Services.AddMemoryCache();
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Để tương thích với Azure
            });

            // API Explorer
            builder.Services.AddEndpointsApiExplorer();

            // CORS Configuration - Cải tiến cho Azure
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", corsBuilder =>
                {
                    if (builder.Environment.IsDevelopment())
                    {
                        // Development: Cho phép localhost
                        corsBuilder
                            .WithOrigins("http://localhost:8080", "https://localhost:8080")
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials();
                    }
                    else
                    {
                        // Production: Cấu hình cụ thể
                        corsBuilder
                            .WithOrigins(
                                "https://fashionhub.name.vn",
                                "https://fashionhub2-bqdrh8fxbwd4ghgd.indonesiacentral-01.azurewebsites.net",
                                "https://fashionhubvns.netlify.app"
                            )
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials();
                    }
                });
            });

            // JWT Authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                };

                // SignalR support
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddAuthorization();

            // Swagger Configuration - Enable cho cả Development và Production
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Nhập token của bạn vào đây (Bearer <your_token>)"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });

                // API Information
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "UltraStore API",
                    Description = "API Documentation for UltraStore Application",
                    Contact = new OpenApiContact
                    {
                        Name = "UltraStore Team",
                        Email = "support@ultrastore.com"
                    }
                });

                // XML Documentation (chỉ nếu file tồn tại)
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline

            // Error handling
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // Không dùng HSTS trên Azure App Service
                // app.UseHsts();
            }

            // Swagger - Enable cho cả Development và Production
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "UltraStore API V1");
                c.RoutePrefix = "swagger";
                c.DocumentTitle = "UltraStore API Documentation";

                // Customization cho Production
                if (app.Environment.IsProduction())
                {
                    c.DefaultModelsExpandDepth(-1); // Hide models section by default
                    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None); // Collapse all sections

                    // Add production warning banner
                    c.HeadContent += @"
                        <style>
                            .swagger-ui .topbar { 
                                background-color: #dc3545 !important; 
                            }
                            .swagger-ui .topbar::after {
                                content: '⚠️ PRODUCTION ENVIRONMENT - USE WITH CAUTION';
                                color: white;
                                font-weight: bold;
                                position: absolute;
                                right: 20px;
                                top: 15px;
                                font-size: 14px;
                            }
                        </style>";
                }
            });

            // Forwarded Headers (quan trọng cho Azure)
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            // Health check endpoint
            app.MapGet("/", async () =>
            {
                try
                {
                    return Results.Ok(new
                    {
                        status = "OK",
                        message = "UltraStore API is running",
                        environment = app.Environment.EnvironmentName,
                        timestamp = DateTime.UtcNow,
                        swagger_url = "/swagger/index.html",
                        health_check_url = "/health"
                    });
                }
                catch (Exception ex)
                {
                    app.Logger.LogError(ex, "Health check failed");
                    return Results.Problem("Health check failed");
                }
            });

            // Thêm health check endpoint cho Azure
            app.MapGet("/health", () => Results.Ok(new
            {
                status = "healthy",
                environment = app.Environment.EnvironmentName,
                timestamp = DateTime.UtcNow
            }));

            // Static files (kiểm tra thư mục tồn tại)
            var uploadsPath = Path.Combine(app.Environment.WebRootPath ?? "wwwroot", "uploads", "chat");
            if (Directory.Exists(uploadsPath))
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
                    RequestPath = "/uploads/chat",
                    OnPrepareResponse = ctx =>
                    {
                        var origin = ctx.Context.Request.Headers["Origin"].FirstOrDefault();
                        if (!string.IsNullOrEmpty(origin))
                        {
                            ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", origin);
                            ctx.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET");
                            ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers", "Authorization");
                        }
                    }
                });
            }
            else
            {
                // Tạo thư mục uploads nếu không tồn tại
                try
                {
                    Directory.CreateDirectory(uploadsPath);
                    app.Logger.LogInformation("Created uploads directory: {Path}", uploadsPath);
                }
                catch (Exception ex)
                {
                    app.Logger.LogWarning(ex, "Could not create uploads directory: {Path}", uploadsPath);
                }
            }

            // HTTPS Redirection - Chỉ trong môi trường phát triển hoặc khi có HTTPS
            if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("ForceHttps"))
            {
                app.UseHttpsRedirection();
            }

            app.UseRouting();

            // CORS - Đặt trước Authentication
            app.UseCors("AllowAll");

            // Session
            app.UseSession();

            // Authentication & Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Custom Middlewares
            app.UseTokenBlacklist();
            app.UseMiddleware<RestrictAdminAccessMiddleware>();

            // Map Controllers và Hubs
            app.MapControllers();

            // SignalR Hubs
            app.MapHub<ChatHub>("/chatHub");
            app.MapHub<Hubs.LienHeHub>("/lienHeHub");
            app.MapHub<Hubs.GiaoDienHub>("/giaoDienHub");

            // Test database connection (optional - for debugging)
            try
            {
                using var scope = app.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await dbContext.Database.CanConnectAsync();
                app.Logger.LogInformation("Database connection successful!");
            }
            catch (Exception ex)
            {
                app.Logger.LogWarning(ex, "Database connection failed: {Message}", ex.Message);
                // Không throw exception để app vẫn có thể start
            }

            // Start the application
            try
            {
                if (app.Environment.IsDevelopment())
                {
                    app.Logger.LogInformation("Starting UltraStore API in Development mode...");
                    app.Logger.LogInformation("Swagger UI available at: http://localhost:{Port}",
                        app.Configuration["ASPNETCORE_URLS"] ?? "https://localhost:7000");
                }
                else
                {
                    app.Logger.LogInformation("Starting UltraStore API on port 8080...");
                    app.Logger.LogInformation("Swagger UI available at: /swagger/index.html");
                }

                app.Logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);

                // Log all registered routes for debugging (chỉ trong Development)
                if (app.Environment.IsDevelopment())
                {
                    var routes = app.Services.GetService<Microsoft.AspNetCore.Routing.EndpointDataSource>();
                    if (routes != null)
                    {
                        foreach (var endpoint in routes.Endpoints)
                        {
                            app.Logger.LogInformation("Route: {DisplayName}", endpoint.DisplayName);
                        }
                    }
                }
                app.Run();
            }
            catch (Exception ex)
            {
                app.Logger.LogCritical(ex, "Application failed to start: {Message}", ex.Message);
                throw;
            }
        }
    }
}