namespace ProductCatalog.Application.Common.Interfaces;

using System.Data;

public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync();
}
