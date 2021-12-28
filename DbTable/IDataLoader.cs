using Microsoft.EntityFrameworkCore;

namespace DbTable;

public interface IDataLoader<DataType> : IDisposable
{
    IQueryable<DataType> GetQuery();
    void EndQuery();
}
