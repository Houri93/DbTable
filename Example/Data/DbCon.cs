using Microsoft.EntityFrameworkCore;

namespace Example.Data;

public class DbCon : DbContext
{
    public DbSet<Item> Items { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {optionsBuilder.UseSqlServer(@$"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=DbTableExampleDB;Integrated Security=True;");
        base.OnConfiguring(optionsBuilder);
    }
}
