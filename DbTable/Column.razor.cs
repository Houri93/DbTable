using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

using System.Linq.Expressions;

namespace DbTable;

[CascadingTypeParameter(nameof(DataType))]
public partial class Column<DataType>
{
    private Func<DataType, object> compiledProperty;

    [Parameter]
    [EditorRequired]
    public string Title { get; set; }

    [Parameter]
    public Expression<Func<DataType, object>> Property { get; set; }

    internal Func<DataType, object> CompiledProperty
    {
        get => compiledProperty ??= Property.Compile();
        set => compiledProperty = value;
    }

    [CascadingParameter(Name = nameof(Table))]
    public Table<DataType> Table { get; set; }

    internal SortMode SortMode { get; set; } = SortMode.Disabled;

    [Parameter]
    public RenderFragment<DataType> Template { get; set; }

    protected override void OnInitialized()
    {
        Table.AddColumn(this);
    }

    private void Clicked()
    {
        SortMode = SortMode.Next();
        Table.ColumnClicked(this);
    }

}