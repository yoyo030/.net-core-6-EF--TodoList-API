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

//三種不同方式去注入
//每次注入時,都是一個新的實例
builder.Services.AddSingleton<SingletonService>();
//每個Request為同一個新的實例
builder.Services.AddScoped<ScopedService>();
//程式運行期間只會有一個實例
builder.Services.AddTransient<TransientService>();
//IOC DI注入
//前面是介面後面是實現方式
builder.Services.AddScoped<ITodoListService, TodoLinqService>();
builder.Services.AddScoped<ITodoListService, TodoLinqService2>();//練習多個實作所以才多加這個注入

builder.Services.AddControllers();

builder.Services.AddControllers().AddNewtonsoftJson();

//登入後 每個class都可以取的使用者資訊
builder.Services.AddHttpContextAccessor();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddSwaggerGen();
//我自己加的 swagger jwt ui
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

////加入Cookie驗證
//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(option =>
//{
//    未登入時會自動導到這個網址
//    option.LoginPath = new PathString("/api/Login/NoLogin");
//    沒權限會導到這邊
//    option.AccessDeniedPath = new PathString("/api/Login/NoAccess");

//    option.ExpireTimeSpan = TimeSpan.FromSeconds(2);
//});

//加入JWT驗證
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
            ClockSkew =TimeSpan.Zero,//過期的時間 緩衝時間為0
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:KEY"]))
        };
    });

//全部都家驗證 有加[AllowAnonymous]才不用
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

//順序要一樣
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();



//加入靜態檔案
app.UseStaticFiles();

app.MapControllers();

app.Run();
