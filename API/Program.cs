
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Domain.Models;
using Domain.Options;
using API.Hubs;
using API.Middleware;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Infrastructure;
using Infrastructure.Repositories;
using Application.Repositories;
using Application.Services.Contract;
using Application.Services.Implementation;
using Serilog;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Bootstrap logger: catches anything that goes wrong before the full
            // Serilog pipeline (which needs configuration/services) is wired up below.
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            try
            {
                RunApp(args);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void RunApp(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day));

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            //  Fireworks config
            builder.Services.AddOptions<FireworksOptions>()
                .Bind(builder.Configuration.GetSection(FireworksOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services.AddSingleton<Fireworksembeddinggenerator>();
            builder.Services.AddSingleton<FireWorkAiChat>();
            builder.Services.AddSingleton<RagService>();

            //  MongoDB config
            builder.Services.AddOptions<MongoOptions>()
                .Bind(builder.Configuration.GetSection(MongoOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services.AddSingleton<IMongoClient>(sp =>
                new MongoClient(sp.GetRequiredService<IOptions<MongoOptions>>().Value.RagDbConnection));
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("PlatformDBContext"));
            });

            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            builder.Services.AddScoped<IClassService, ClassService>();
            builder.Services.AddScoped<ITrackService, TrackService>();
            builder.Services.AddScoped<ISubjectService, SubjectService>();
            builder.Services.AddScoped<IUnitService, UnitService>();
            builder.Services.AddScoped<ILessonService, LessonService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<IQuizService, QuizService>();

            builder.Services.Configure<FileUploadOptions>(builder.Configuration.GetSection(FileUploadOptions.SectionName));
            builder.Services.AddScoped<IFileService, FileService>();

            builder.Services.AddOptions<PaymobOptions>()
                .Bind(builder.Configuration.GetSection(PaymobOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services.AddOptions<JwtOptions>()
                .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();



            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 100_000_000;
            });

            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<AppDbContext>();


            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                //[authrize]
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;//
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>//verified key
            {
                var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
                    ?? throw new InvalidOperationException("JWT configuration section is missing.");

                options.SaveToken = true;
                options.RequireHttpsMetadata = true;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidAudience = jwtOptions.AudienceIP,
                    ValidIssuer = jwtOptions.IssuerIP,
                    IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SCRKey)),
                    //RoleClaimType = "role"
                };
            });

            //builder.Services.AddAuthorization();



            builder.Services.AddSignalR();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("FreePlan", policy =>
                {
                    policy.WithOrigins("https://localhost:4200", "http://localhost:4200").AllowAnyHeader().AllowAnyMethod().AllowCredentials(); ;
                });
            });



            /*-----------------------------Swagger PArt-----------------------------*/
            #region Swagger Region
            ////builder.Services.AddSwaggerGen();

            //builder.Services.AddSwaggerGen(swagger =>
            //{
            //    //This�is�to�generate�the�Default�UI�of�Swagger�Documentation����
            //    swagger.SwaggerDoc("v1", new OpenApiInfo
            //    {
            //        Version = "v1",
            //        Title = "ASP.NET�5�Web�API",
            //        Description = " ITI Projrcy"
            //    });
            //    //�To�Enable�authorization�using�Swagger�(JWT)����
            //    swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            //    {
            //        Name = "Authorization",
            //        Type = SecuritySchemeType.ApiKey,
            //        Scheme = "Bearer",
            //        BearerFormat = "JWT",
            //        In = ParameterLocation.Header,
            //        Description = "Enter�'Bearer'�[space]�and�then�your�valid�token�in�the�text�input�below.\r\n\r\nExample:�\"Bearer�eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\"",
            //    });
            //    swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
            //    {
            //        {
            //        new OpenApiSecurityScheme
            //        {
            //        Reference = new OpenApiReference
            //        {
            //        Type = ReferenceType.SecurityScheme,
            //        Id = "Bearer"
            //        }
            //        },
            //        new string[] {}
            //        }
            //        });
            //});
            #endregion

            var app = builder.Build();

            app.UseSerilogRequestLogging();

            app.UseExceptionHandler();

            app.UseStaticFiles();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("FreePlan");
            app.UseAuthentication(); 
            app.UseAuthorization();




            app.MapHub<ChatHub>("/chat");

            app.MapControllers();

            app.Run();
        }
    }
}
