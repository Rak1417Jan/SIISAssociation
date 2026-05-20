using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVEA.Repository.Infrastructure
{
    public interface IConnectionFactory : IDisposable
    {
        IDbConnection GetConnection();
    }
    public interface ISqlConnectionFactory : IConnectionFactory
    {

    }
}
