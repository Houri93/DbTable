using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

using System.Linq.Expressions;

namespace DbTable;

public partial class Table<DbContextType, DataType>
    where DataType : class
    where DbContextType : DbContext
{
    private readonly List<DataType> items = new();
    private readonly List<Column<DbContextType, DataType>> columns = new();
    private int pageIndex = 0;
    private int pageCount = 0;
    private int itemsCount = 0;

    [Parameter]
    [EditorRequired]
    public int ItemsPerPage { get; set; }

    [Parameter]
    [EditorRequired]
    public IDataLoader<DbContextType, DataType> DataLoader { get; set; }

    [Parameter]
    public string Class { get; set; }

    [Parameter]
    public RenderFragment ChildContent { get; set; }

    internal bool FirstPageEnabled => pageIndex > 0;
    internal bool NextPageEnabled => pageIndex + 1 <= pageCount - 1;
    internal bool PrevPageEnabled => pageIndex - 1 >= 0;
    internal bool LastPageEnabled => pageIndex < pageCount - 1;


    private void FirstPageClicked() => SetPageIndex(0);
    private void NextPageClicked() => SetPageIndex(pageIndex + 1);
    private void PrevPageClicked() => SetPageIndex(pageIndex - 1);
    private void LastPageClicked() => SetPageIndex(pageCount - 1);

    private void SetPageIndex(int i)
    {
        pageIndex = i;
        UpdateData();
    }

    internal void AddColumn(Column<DbContextType, DataType> column)
    {
        columns.Add(column);
        OnParametersSet();
    }
    internal void ColumnClicked(Column<DbContextType, DataType> column)
    {
        var other = columns.Where(a => a != column);

        foreach (var c in other)
        {
            c.SortMode = SortMode.Disabled;
        }

        pageIndex = 0;
        OnParametersSet();
    }
    private void UpdateData()
    {
        var db = DataLoader.GetDbContext();
        var dbSet = DataLoader.GetDbSet(db);
        itemsCount = dbSet.Count();

        pageCount = itemsCount / ItemsPerPage;

        var query = dbSet.AsQueryable();

        var sortColumn = columns.FirstOrDefault(a => a.SortMode != SortMode.Disabled);
        if (sortColumn is not null)
        {
            var prop = sortColumn.Property;

            if (sortColumn.SortMode == SortMode.Descending)
            {
                query = query.OrderByDescending(prop);
            }
            else
            {
                query = query.OrderBy(prop);
            }
        }

        query = query.Skip(ItemsPerPage * pageIndex).Take(ItemsPerPage);

        items.Clear();
        items.AddRange(query.AsEnumerable());
    }

    protected override void OnParametersSet()
    {
        UpdateData();
        InvokeAsync(StateHasChanged);
    }
}
