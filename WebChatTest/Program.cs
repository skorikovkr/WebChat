using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebChatTest.Hubs;
using WebChatTest.Models.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options
        .UseLazyLoadingProxies()
        .UseSqlServer(connectionString));
builder.Services.AddIdentity<AppUser, IdentityRole>(opt =>
{
    opt.Password.RequireDigit = false;
    opt.Password.RequireLowercase = false;
    opt.Password.RequireUppercase = false;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequiredLength = 1;
    opt.User.RequireUniqueEmail = false;
    opt.SignIn.RequireConfirmedEmail = false;
}).AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddCors(options => options.AddPolicy("CorsPolicy",
        builder =>
        {
            builder.AllowAnyHeader()
                   .AllowAnyMethod()
                   .SetIsOriginAllowed((host) => true)
                   .AllowCredentials();
        }));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CorsPolicy");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chat");
app.Run();
