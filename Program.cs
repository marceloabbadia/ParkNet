using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjParkNet.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("ParkingConnection") ?? throw new InvalidOperationException("Connection string 'ParkingConnection' not found.");
builder.Services.AddDbContext<ParkingDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>() 
    .AddEntityFrameworkStores<ParkingDbContext>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddRazorPages();
builder.Services.AddScoped<ParkingRepository>();
builder.Services.AddScoped<ParkingSpotsRepository>();
builder.Services.AddScoped<ParkingUsageRepository>();


var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await CreateRolesAsync(services);
}

app.Run();



async Task CreateRolesAsync(IServiceProvider serviceProvider)
{
    using (var scope = serviceProvider.CreateScope())
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        // Criar papel Admin se não existir
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
            Console.WriteLine("Created Role Admin.");
        }

        // Buscar usuário administrador pelo email
        string adminEmail = "marcelo@teste.com.br";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser != null)
        {
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                Console.WriteLine($"User {adminEmail} Added Admin.");
            }
            else
            {
                Console.WriteLine($"The User {adminEmail} is Admin.");
            }
        }
        else
        {
            // Criar um novo admin se o usuário não existir
            var newAdminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
               
            };

            var createAdminResult = await userManager.CreateAsync(newAdminUser, "123456@");

            if (createAdminResult.Succeeded)
            {
                await userManager.AddToRoleAsync(newAdminUser, "Admin");
                Console.WriteLine($"Created Admin User {adminEmail}.");
            }
            else
            {
                Console.WriteLine($"Failed to create user {adminEmail}: {string.Join(", ", createAdminResult.Errors.Select(e => e.Description))}");
            }
        }
    }
}
