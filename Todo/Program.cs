using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Todo.Interfaces;
using Todo.Models;
using Todo.Services;

var builder = WebApplication.CreateBuilder(args);

//AddConnectionString
var connectString = builder.Configuration.GetConnectionString("TodoDatabase");

// Add services to the container.
builder.Services.AddDbContext<TodoContext>(x => x.UseSqlServer(connectString));
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScoped<TodoListService>();
builder.Services.AddScoped<TestDIService>();

//�T�ؤ��P�覡�h�`�J
//�C���`�J��,���O�@�ӷs�����
builder.Services.AddSingleton<SingletonService>();
//�C��Request���P�@�ӷs�����
builder.Services.AddScoped<ScopedService>();
//�{���B������u�|���@�ӹ��
builder.Services.AddTransient<TransientService>();
//IOC DI�`�J
//�e���O�����᭱�O��{�覡
builder.Services.AddScoped<ITodoListService, TodoLinqService>();
builder.Services.AddScoped<ITodoListService, TodoLinqService2>();//�m�ߦh�ӹ�@�ҥH�~�h�[�o�Ӫ`�J

builder.Services.AddControllers();

builder.Services.AddControllers().AddNewtonsoftJson();

//�n�J�� �C��class���i�H�����ϥΪ̸�T
builder.Services.AddHttpContextAccessor();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddSwaggerGen();
//�ڦۤv�[�� swagger jwt ui
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

////�[�JCookie����
//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(option =>
//{
//    ���n�J�ɷ|�۰ʾɨ�o�Ӻ��}
//    option.LoginPath = new PathString("/api/Login/NoLogin");
//    �S�v���|�ɨ�o��
//    option.AccessDeniedPath = new PathString("/api/Login/NoAccess");

//    option.ExpireTimeSpan = TimeSpan.FromSeconds(2);
//});

//�[�JJWT����
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew =TimeSpan.Zero,//�L�����ɶ� �w�Įɶ���0
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:KEY"]))
        };
    });

//�������a���� ���[[AllowAnonymous]�~����
builder.Services.AddMvc(options =>
{
    options.Filters.Add(new AuthorizeFilter());
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//���ǭn�@��
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();



//�[�J�R�A�ɮ�
app.UseStaticFiles();

app.MapControllers();

app.Run();
