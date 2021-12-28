using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

using System.Linq.Expressions;
using System.Reflection;

namespace DbTable;

[CascadingTypeParameter(nameof(DataType))]
public partial class Column<DataType>
{
    private Func<DataType, object> compiledProperty;

    internal decimal? FilterNumber { get; set; }
    internal DateOnly? FilterDate { get; set; }
    internal TimeOnly? FilterTime { get; set; }
    internal DateTime? FilterDateTime { get; set; }
    internal string FilterString { get; set; }
    internal bool FilterBool { get; set; }

    [Parameter]
    [EditorRequired]
    public string Title { get; set; }

    [Parameter]
    public bool Sort { get; set; } = false;

    [Parameter]
    public bool Filter { get; set; }
    internal FilterTypes FilterType { get; set; } = FilterTypes.None;

    [Parameter]
    [EditorRequired]
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

    internal string PropertyName { get; set; }
    internal Type PropertyType { get; set; }
    internal PropertyInfo PropertyInfo { get; set; }

    protected override void OnInitialized()
    {
        AssignFilterType();

        Table.AddColumn(this);
    }

    private void AssignFilterType()
    {
        if (!Filter)
        {
            return;
        }

        var body = Property.Body;
        if (body is UnaryExpression unary)
        {
            PropertyInfo = (PropertyInfo)((MemberExpression)unary.Operand).Member;
        }
        else
        {
            PropertyInfo = (PropertyInfo)((MemberExpression)body).Member;
        }

        PropertyName = PropertyInfo.Name;
        PropertyType = PropertyInfo.PropertyType;

        if (PropertyType == typeof(int) ||
            PropertyType == typeof(long) ||
            PropertyType == typeof(uint) ||
            PropertyType == typeof(short) ||
            PropertyType == typeof(ushort) ||
            PropertyType == typeof(ulong) ||
            PropertyType == typeof(byte))
        {
            FilterType = FilterTypes.IntNumber;
        }
        else if (PropertyType == typeof(float) ||
            PropertyType == typeof(double) ||
            PropertyType == typeof(decimal))
        {
            FilterType = FilterTypes.DoubleNumber;
        }
        else if (PropertyType == typeof(bool))
        {
            FilterType = FilterTypes.Bool;
        }
        else if (PropertyType == typeof(DateOnly))
        {
            FilterType = FilterTypes.DateOnly;
        }
        else if (PropertyType == typeof(TimeOnly))
        {
            FilterType = FilterTypes.TimeOnly;
        }
        else if (PropertyType == typeof(DateTime))
        {
            FilterType = FilterTypes.DateTime;
        }
        else if (PropertyType.IsEnum)
        {
            FilterType = FilterTypes.Enum;
        }
        else
        {
            FilterType = FilterTypes.String;
        }
    }

    private void Clicked()
    {
        if (!Sort)
        {
            return;
        }
        SortMode = SortMode.Next();
        Table.ColumnClicked(this);
    }

}