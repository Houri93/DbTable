using Example.Data;
using Example.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Internal;

namespace Example.Pages;

public partial class FetchData : IDisposable
{
    private ItemLoader itemLoader = new();

    [Inject]
    public ItemService ItemService { get; set; }


    protected override void OnInitialized()
    {
        ItemService.Add += ItemService_Add;
        base.OnInitialized();
    }

    private void ItemService_Add()
    {
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        ItemService.Add -= ItemService_Add;

        itemLoader.Dispose();
        itemLoader = null;
    }

}
