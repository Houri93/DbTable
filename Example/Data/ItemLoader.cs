using DbTable;

using Microsoft.EntityFrameworkCore;

namespace Example.Data;

public class ItemLoader : IDataLoader<DbCon, Item>
{
    public DbCon GetDbContext()
    {
        return new DbCon();
    }

    public DbSet<Item> GetDbSet(DbCon db)
    {
        return db.Items;
    }

    public void DisposeDbContext(DbCon db)
    {
        db.Dispose();
    }
}
