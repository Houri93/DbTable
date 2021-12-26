using Example.Data;

using Microsoft.EntityFrameworkCore.Design.Internal;

namespace Example.Services;

public class ItemService
{

    public event Action Add;

    public ItemService()
    {
        using var db = new DbCon();
        if (!db.Items.Any())
        {
            db.Items.AddRange(Enumerable.Range(1, 10).Select(i => new Item
            {
                Created = DateTime.Now,
                Name = "Item " + i.ToString(),
            }));
            db.SaveChanges();
        }

        RunGenerator();
    }

    private void RunGenerator()
    {
        Task.Run(async () =>
        {
            while (true)
            {
                using var db = new DbCon();
                db.Items.Add(new Item
                {
                    Created = DateTime.Now,
                    Name = "Item " + db.Items.Count(),
                });
                db.SaveChanges();
                Add?.Invoke();
                await Task.Delay(1000);
            }
        });
    }
}
