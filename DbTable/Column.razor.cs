using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace DbTable;

public partial class Column<DbContextType, DataType>
    where DataType : class
    where DbContextType : DbContext
{
    private bool sortAsc = false;

    [Parameter]
    [EditorRequired]
    public string Title { get; set; }

    [Parameter]
    public Func<DataType, object> Filter { get; set; }

    [Parameter]
    public Func<DataType, object> Sort { get; set; }

    [CascadingParameter]
    public Table<DbContextType, DataType> Table { get; set; }
}