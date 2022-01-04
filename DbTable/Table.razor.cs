using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

using System.Linq.Dynamic.Core.Parser;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace DbTable;

[CascadingTypeParameter(nameof(DataType))]
public partial class Table<DataType>
{
    const int displayedPageCount = 2;

    private readonly List<DataType> items = new();
    private readonly List<Column<DataType>> columns = new();
    private int pageIndex = 0;
    private int pageCount = 0;
    private int itemsCount = 0;
    private int queryTotalCount = 0;
    private int pageSelectStart = 1;
    private int pageSelectEnd = 1;

    [Parameter]
    [EditorRequired]
    public int ItemsPerPage { get; set; }


    [Parameter]
    public bool ShowItemsCount { get; set; } = true;   

    [Parameter]
    public bool ShowPageCount { get; set; } = true;

    [Parameter]
    [EditorRequired]
    public IDataLoader<DataType> DataLoader { get; set; }

    [Parameter]
    public string Class { get; set; }

    [Parameter]
    public RenderFragment ChildContent { get; set; }

    internal bool FirstPageEnabled => queryTotalCount > 0 && pageIndex > 0;
    internal bool NextPageEnabled => queryTotalCount > 0 && pageIndex + 1 <= pageCount - 1;
    internal bool PrevPageEnabled => queryTotalCount > 0 && pageIndex - 1 >= 0;
    internal bool LastPageEnabled => queryTotalCount > 0 && pageIndex < pageCount - 1;

    internal IEnumerable<Column<DataType>> FilterableColumns => columns.Where(a => a.Filter);

    private void FirstPageClicked() => SetPageIndex(0);
    private void NextPageClicked() => SetPageIndex(pageIndex + 1);
    private void PrevPageClicked() => SetPageIndex(pageIndex - 1);
    private void LastPageClicked() => SetPageIndex(pageCount - 1);

    private void SetPageIndex(int i)
    {
        if (i < 0)
        {
            return;
        }

        if (i >= pageCount)
        {
            return;
        }

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
    private void FilterClicked()
    {
        pageIndex = 0;
        UpdateData();
    }
    private void FilterClearClicked()
    {
        pageIndex = 0;
        foreach (var c in FilterableColumns)
        {
            c.FilterString = default;

            c.FilterDateFrom = default;
            c.FilterDateTo = default;

            c.FilterTimeFrom = default;
            c.FilterTimeTo = default;

            c.FilterDateTimeFrom = default;
            c.FilterDateTimeTo = default;

            c.FilterNumberFrom = default;
            c.FilterNumberTo = default;
        }
        UpdateData();
    }
    private void UpdateData()
    {
        var query = DataLoader.GetQuery();
        itemsCount = query.Count();

        query = ApplyFilters(query);
        query = ApplyOrdering(query);

        queryTotalCount = query.Count();

        pageCount = (int)Math.Ceiling(queryTotalCount / (double)ItemsPerPage);

        if (pageCount == 0)
        {
            pageSelectStart = pageSelectEnd = 1;
        }
        else
        {
            var pages = Enumerable
            .Range(PageLimits(), displayedPageCount * 2 + 1)
            .TakeWhile(p => p <= pageCount)
            .ToList();

            pageSelectStart = pages.First();
            pageSelectEnd = pages.Last();
        }

        query = query.Skip(ItemsPerPage * pageIndex).Take(ItemsPerPage);

        var inMemory = query.ToList();

        items.Clear();
        items.AddRange(inMemory);
    }

    private int PageLimits()
    {
        return Math.Min(Math.Max(1, pageCount - (2 * displayedPageCount)), Math.Max(1, pageIndex - displayedPageCount));
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
                FilterTypes.IntNumber => ApplySingleFilter(column.FilterNumberFrom, column.FilterNumberTo, pi, query, true, false, true),
                FilterTypes.DoubleNumber => ApplySingleFilter(column.FilterNumberFrom, column.FilterNumberTo, pi, query, true, false, true),
                FilterTypes.String => ApplySingleFilter(column.FilterString, null, pi, query, false, true, false),
                FilterTypes.DateOnly => ApplySingleFilter(column.FilterDateFrom, column.FilterDateTo, pi, query, false, false, true),
                FilterTypes.TimeOnly => ApplySingleFilter(column.FilterTimeFrom, column.FilterTimeTo, pi, query, false, false, true),
                FilterTypes.DateTime => ApplySingleFilter(column.FilterDateTimeFrom, column.FilterDateTimeTo, pi, query, false, false, true),
                FilterTypes.Bool => ApplySingleFilter(column.FilterString is null ? (bool?)null : bool.Parse(column.FilterString), null, pi, query, false, false, false),
                FilterTypes.Enum => ApplySingleFilter(column.FilterString is null ? null : Enum.Parse(column.PropertyType, column.FilterString), null, pi, query, false, false, false),
                _ => throw new NotImplementedException("Filter type not implemented"),
            };
        }

        return query;
    }

    private IQueryable<DataType> ApplySingleFilter<T>(T? fromValue, T? toValue, PropertyInfo pi, IQueryable<DataType> query, bool castSourceToDecimal, bool contains, bool range)
    {
        if (fromValue is null && toValue is null)
        {
            return query;
        }

        var param = Expression.Parameter(typeof(DataType), "a");
        var prop = Expression.Property(param, pi);

        var fromValueExp = Expression.Constant(fromValue);
        var toValueExp = Expression.Constant(toValue);


        Expression src = castSourceToDecimal ? Expression.Convert(prop, typeof(decimal)) : prop;

        Expression src2;
        if (contains)
        {
            var valueLower = Expression.Call(fromValueExp, "ToLower", null, null);
            var srcLower = Expression.Call(src, "ToLower", null, null);
            src2 = Expression.Call(srcLower, "Contains", null, valueLower);
        }
        else if (range)
        {
            if (fromValue is not null && toValue is not null)
            {
                var less = Expression.LessThanOrEqual(fromValueExp, src);
                var greater = Expression.GreaterThanOrEqual(toValueExp, src);
                var and = Expression.And(less, greater);
                src2 = and;
            }
            else if (fromValue is not null && toValue is null)
            {
                var less = Expression.LessThanOrEqual(fromValueExp, src);
                src2 = less;
            }
            else
            {
                var greater = Expression.GreaterThanOrEqual(toValueExp, src);
                src2 = greater;
            }
        }
        else
        {
            src2 = Expression.Equal(src, fromValueExp);
        }

        var lamda = Expression.Lambda(src2, param);

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
