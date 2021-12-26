using DbTable;

using Microsoft.EntityFrameworkCore;

namespace Example.Data;

public class ItemLoader : IDataLoader<Item>
{
    private DbCon db;



    public IQueryable<Item> GetQuery()
    {
        db = new DbCon();
        return db.Items;
    }
    public void EndQuery()
    {
        db.Dispose();
    }

}
