using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

using System.Linq.Expressions;

namespace DbTable;

public partial class Column<DbContextType, DataType>
    where DataType : class
    where DbContextType : DbContext
{


    [Parameter]
    [EditorRequired]
    public string Title { get; set; }

    [Parameter]
    public Expression<Func<DataType, object>> Property { get; set; }

    [CascadingParameter]
    public Table<DbContextType, DataType> Table { get; set; }

    internal SortMode SortMode { get; set; } = SortMode.Disabled;

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            Table.AddColumn(this);
        }
    }

    private void Clicked()
    {
        SortMode = SortMode.Next();
        Table.ColumnClicked(this);
    }

}