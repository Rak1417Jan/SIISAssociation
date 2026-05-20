using Microsoft.Extensions.Logging;
using MVEA.Repository.Infrastructure;
using MVEA.Repository.Interfaces;
using MVEA.Repository.IRepository;
using MVEA.Repository.Repositories;
using MVEA.Repository.Repository;


namespace MVEA.Repository.UnitOfWork
{
    public class UnitOFWork : IUnitOfWork
    {
        private readonly ISqlConnectionFactory _connection;
        private readonly ILogger<AuthRepository> _loggerAuth;
        private readonly ILogger<UserRepository> _loggerUser;
        private readonly ILogger<MasterRepository> _loggerMaster;
        public UnitOFWork(
            ISqlConnectionFactory connection,
            ILogger<AuthRepository> logger,
            ILogger<UserRepository> loggerUser,
            ILogger<MasterRepository> loggerMaster)
        {
            _connection = connection;
            _loggerAuth = logger;
            _loggerUser = loggerUser;
            _loggerMaster = loggerMaster;
        }

        public IAuthRepository AuthRepository
        {
            get
            {
                return new AuthRepository(_connection, _loggerAuth);
            }
        }
        public IUserRepository UserRepository
        {
            get
            {
                return new UserRepository(_connection, _loggerUser);
            }
        }
        public IMasterRepository MasterRepository
        {
            get
            {
                return new MasterRepository(_connection, _loggerMaster);
            }
        }
    }
}
