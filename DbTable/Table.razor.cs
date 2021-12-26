using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace DbTable;

public partial class Table<DbContextType, DataType>
    where DataType : class
    where DbContextType : DbContext
{
    [Parameter]
    [EditorRequired]
    public int ItemsPerPage { get; set; }

    [Parameter]
    [EditorRequired]
    public IDataLoader<DbContextType, DataType> DataLoader { get; set; }

    [Parameter]
    public RenderFragment ChildContent { get; set; }
}
