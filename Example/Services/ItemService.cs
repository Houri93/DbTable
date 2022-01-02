using Example.Data;

using Microsoft.EntityFrameworkCore.Design.Internal;

namespace Example.Services;

public class ItemService
{

    public event Action Add;

    public ItemService()
    {      
        RunGenerator();
    }

    private void RunGenerator()
    {
        Task.Run(async () =>
        {
            var httpClient = new HttpClient();
            while (true)
            {
                using var db = new DbCon();

                db.Items.Add(new Item
                {
                    Created = DateTime.Now,
                    Name = "Item " + db.Items.Count(),
                    Image = Convert.ToBase64String(await httpClient.GetByteArrayAsync("https://picsum.photos/200")),
                });
                db.SaveChanges();
                Add?.Invoke();
                await Task.Delay(1000);
            }
        });
    }
}
