using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace DbTable;

[CascadingTypeParameter(nameof(DataType))]
public partial class Table<DataType>
{
    private readonly List<DataType> items = new();
    private readonly List<Column<DataType>> columns = new();
    private int pageIndex = 0;
    private int pageCount = 0;
    private int itemsCount = 0;

    [Parameter]
    [EditorRequired]
    public int ItemsPerPage { get; set; }

    [Parameter]
    [EditorRequired]
    public IDataLoader<DataType> DataLoader { get; set; }

    [Parameter]
    public string Class { get; set; }

    [Parameter]
    public RenderFragment ChildContent { get; set; }

    internal bool FirstPageEnabled => pageIndex > 0;
    internal bool NextPageEnabled => pageIndex + 1 <= pageCount - 1;
    internal bool PrevPageEnabled => pageIndex - 1 >= 0;
    internal bool LastPageEnabled => pageIndex < pageCount - 1;

    internal IEnumerable<Column<DataType>> FilterableColumns => columns.Where(a => a.Filter);


    private void FirstPageClicked() => SetPageIndex(0);
    private void NextPageClicked() => SetPageIndex(pageIndex + 1);
    private void PrevPageClicked() => SetPageIndex(pageIndex - 1);
    private void LastPageClicked() => SetPageIndex(pageCount - 1);

    private void SetPageIndex(int i)
    {
        pageIndex = i;
        UpdateData();
    }

    internal void AddColumn(Column<DataType> column)
    {
        columns.Add(column);
        OnParametersSet();
    }
    internal void ColumnClicked(Column<DataType> column)
    {
        var other = columns.Where(a => a != column);

        foreach (var c in other)
        {
            c.SortMode = SortMode.Disabled;
        }

        pageIndex = 0;
        OnParametersSet();
    }
    private void SearchClicked()
    {
        UpdateData();
    }
    private void UpdateData()
    {
        var query = DataLoader.GetQuery();
        itemsCount = query.Count();
        pageCount = itemsCount / ItemsPerPage;

        query = ApplyFilters(query);
        query = ApplyOrdering(query);

        query = query.Skip(ItemsPerPage * pageIndex).Take(ItemsPerPage);

        var inMemory = query.ToList();

        items.Clear();
        items.AddRange(inMemory);
    }

    private IQueryable<DataType> ApplyFilters(IQueryable<DataType> query)
    {
        foreach (var column in FilterableColumns)
        {
            var pi = column.PropertyInfo;

            if (string.IsNullOrEmpty(column.FilterString))
            {
                column.FilterString = null;
            }

            query = column.FilterType switch
            {
                FilterTypes.IntNumber => ApplySingleFilter(column.FilterNumber, pi, query, true),
                FilterTypes.DoubleNumber => ApplySingleFilter(column.FilterNumber, pi, query, true),
                FilterTypes.String => ApplySingleFilter(column.FilterString, pi, query, false),
                FilterTypes.DateOnly => ApplySingleFilter(column.FilterDate, pi, query, false),
                FilterTypes.TimeOnly => ApplySingleFilter(column.FilterTime, pi, query, false),
                FilterTypes.DateTime => ApplySingleFilter(column.FilterDateTime, pi, query, false),
                FilterTypes.Bool => ApplySingleFilter(column.FilterBool, pi, query, false),
                FilterTypes.Enum => ApplySingleFilter(column.FilterString is null ? null : Enum.Parse(column.PropertyType, column.FilterString), pi, query, false),
                _ => throw new NotImplementedException("Filter type not implemented"),
            };
        }

        return query;
    }

    private IQueryable<DataType> ApplySingleFilter<T>(T? value, PropertyInfo pi, IQueryable<DataType> query, bool asString)
    {
        if (value is null)
        {
            return query;
        }

        var param = Expression.Parameter(typeof(DataType), "a");
        var prop = Expression.Property(param, pi);
        var valueExp = Expression.Constant(asString ? value.ToString() : value);

        Expression src;
        if (asString)
        {
            src = Expression.Call(prop, "ToString", null);
        }
        else
        {
            src = prop;
        }

        var equals = Expression.Equal(src, valueExp);
        var lamda = Expression.Lambda(equals, param);

        var types = new Type[] { query.ElementType };
        var call = Expression.Call(typeof(Queryable), "Where", types, query.Expression, lamda);
        return query.Provider.CreateQuery<DataType>(call);
    }

    private IQueryable<DataType> ApplyOrdering(IQueryable<DataType> query)
    {
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

        return query;
    }

    protected override void OnParametersSet()
    {
        UpdateData();
        InvokeAsync(StateHasChanged);
    }
}
