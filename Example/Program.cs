
using Example.Data;

using System.Linq;

using (var db = new DbCon())
{
    db.Database.EnsureCreated();

    if (!db.Items.Any())
    {
        db.Items.AddRange(Enumerable.Range(1, 10).Select(i => new Item
        {
            Created = DateTime.Now,
            Name = "Item " + i.ToString(),
        }));
        db.SaveChanges();       
    }
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

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

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
