using EShop_Appication.Catalog.Product;
using EShop_Appication.Common;
using EShop_Appication.UserSystem;
using EShop_Data.EF;
using EShop_Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddDbContext<EShopDBContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyDB"));
});
builder.Services.AddIdentity<AppUser,AppRole>().AddEntityFrameworkStores<EShopDBContext>().AddDefaultTokenProviders();
builder.Services.AddScoped<IStorageService,FileStorageService>();
builder.Services.AddScoped<IPublicProductService,PublicProductService>();
builder.Services.AddScoped<IManageProductService,ManageProductService>();
builder.Services.AddScoped<UserManager<AppUser>,UserManager<AppUser>>();
builder.Services.AddScoped<SignInManager<AppUser>,SignInManager<AppUser>>();
builder.Services.AddScoped<RoleManager<AppRole>,RoleManager<AppRole>>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
