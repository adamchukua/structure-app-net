using Microsoft.EntityFrameworkCore;
using StructureApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<FolderContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DbContext")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapControllerRoute(
    name: "import",
    pattern: "Folder/Import",
    defaults: new { controller = "Folder", action = "Import" });

app.MapControllerRoute(
    name: "default",
    pattern: "{*resource}",
    defaults: new { controller = "Folder", action = "Index" });

app.UseAuthorization();

app.MapRazorPages();

app.Run();
