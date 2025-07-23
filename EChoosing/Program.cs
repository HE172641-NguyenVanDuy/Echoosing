using BusinessObjects.Models;
using DinkToPdf.Contracts;
using DinkToPdf;
using Microsoft.EntityFrameworkCore;
using Services;
using Services.OTP;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);


try
{
    var context = new CustomAssemblyLoadContext();
    context.LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/lib/libwkhtmltox.dll"));
}
catch (Exception ex)
{
    // Ghi log lỗi
    Console.WriteLine($"Error loading libwkhtmltox.dll: {ex.Message}");
    throw;
}

// Add services to the container.

builder.Services.AddDbContext<EchoosingContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionString"))
);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddSingleton<ITempOtpStorage, InMemoryOtpStorage>();


builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

builder.Services.Configure<MailSetting>(builder.Configuration.GetSection("MailSetting"));
builder.Services.AddTransient<SendGmail>();

builder.Services.AddSingleton<IGetEmailTemplateService, GetEmailTemplateServie>();
builder.Services.AddScoped<GetEmailTemplateServie>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<IOptionService, OptionService>();
builder.Services.AddScoped<IExamQuestionService, ExamQuestionService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IExcelImportService, ExcelImportService>();
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<ICodeJoinClassService, CodeJoinClassService>();
builder.Services.AddScoped<IAssignmentService, AssignmentService>();
builder.Services.AddScoped<ITestService, TestService>();
builder.Services.AddScoped<IExportPDFservice, ExportPDFservice>();
builder.Services.AddScoped<ICodeExamService, CodeExamService>();
builder.Services.AddScoped<IQuizletService, QuizletService>();

//builder.Services.AddHostedService<CronJobSentMail>();


builder.Services.AddScoped<ITestService, TestService>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));// hoặc PdfConverter nếu bạn dùng kiểu khác
builder.Services.AddScoped<IExportPDFservice, ExportPDFservice>();
builder.Services.AddScoped<IExamQuestionService, ExamQuestionService>();
builder.Services.AddScoped<IOptionService, OptionService>();
builder.Services.AddScoped<IExcelImportService, ExcelImportService>();
builder.Services.AddScoped<ICodeExamService, CodeExamService>();
builder.Services.AddScoped<ICodeJoinClassService, CodeJoinClassService>();
builder.Services.AddScoped<TokenService>(); // nếu không có interface thì dùng AddScoped trực tiếp
builder.Services.AddScoped<IPasswordService, PasswordService>();

builder.Services.AddEndpointsApiExplorer();



IConfiguration configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", true, true).Build();

builder.Services
.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = false,
        ValidIssuer = configuration["JwtSettings:Issuer"],
        ValidAudience = configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"]))
    };
});

builder.Services.AddSwaggerGen(c =>
{
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "JWT Authentication for Cosmetics Management",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    };

    c.AddSecurityDefinition("Bearer", jwtSecurityScheme);

    var securityRequirement = new OpenApiSecurityRequirement
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
                };
    c.AddSecurityRequirement(securityRequirement);
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policyBuilder => policyBuilder.RequireAssertion(
            context => context.User.HasClaim(claim => claim.Type == "Role") &&
            context.User.FindFirst(claim => claim.Type == "Role").Value == "1"));

    options.AddPolicy("GuestOnly", policyBuilder =>
        policyBuilder.RequireAssertion(
            context => context.User.HasClaim(claim => claim.Type == "Role") &&
            context.User.FindFirst(claim => claim.Type == "Role").Value == "2"));

    options.AddPolicy("StudentOnly", policyBuilder =>
        policyBuilder.RequireAssertion(
            context => context.User.HasClaim(claim => claim.Type == "Role") &&
            context.User.FindFirst(claim => claim.Type == "Role").Value == "3"));

    options.AddPolicy("TeacherOnly", policyBuilder =>
        policyBuilder.RequireAssertion(
            context => context.User.HasClaim(claim => claim.Type == "Role") &&
            context.User.FindFirst(claim => claim.Type == "Role").Value == "4"));

    options.AddPolicy("StudentOrTeacher", policyBuilder =>
        policyBuilder.RequireAssertion(
            context => context.User.HasClaim(claim => claim.Type == "Role") &&
            (context.User.FindFirst(claim => claim.Type == "Role")?.Value == "3" ||
             context.User.FindFirst(claim => claim.Type == "Role")?.Value == "4")));

    options.AddPolicy("StudentOrGuest", policyBuilder =>
        policyBuilder.RequireAssertion(context =>
            context.User.HasClaim(claim => claim.Type == "Role") &&
            (context.User.FindFirst(claim => claim.Type == "Role")?.Value == "1" ||
             context.User.FindFirst(claim => claim.Type == "Role")?.Value == "3")));

    options.AddPolicy("AllRoles", policyBuilder =>
        policyBuilder.RequireAssertion(context =>
            context.User.HasClaim(claim => claim.Type == "Role") &&
            (context.User.FindFirst(claim => claim.Type == "Role")?.Value == "1" ||
            context.User.FindFirst(claim => claim.Type == "Role")?.Value == "2" ||
            context.User.FindFirst(claim => claim.Type == "Role")?.Value == "4" ||
             context.User.FindFirst(claim => claim.Type == "Role")?.Value == "3")));
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthentication();


app.MapControllers();

app.Run();
