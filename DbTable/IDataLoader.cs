using Microsoft.EntityFrameworkCore;

namespace DbTable;

public interface IDataLoader<DbContextType, DataType>
    where DataType : class 
    where DbContextType : DbContext
{
    DbContextType GetDbContext();

    DbSet<DataType> GetDbSet(DbContextType db);

    void DisposeDbContext(DbContextType db);
}
