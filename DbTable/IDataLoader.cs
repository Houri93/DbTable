using Microsoft.EntityFrameworkCore;

namespace DbTable;

public interface IDataLoader<DataType>
{
    IQueryable<DataType> GetQuery();
    void EndQuery();
}
