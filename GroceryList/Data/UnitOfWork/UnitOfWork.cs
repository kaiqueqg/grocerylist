using GroceryList.Data.Caching;
using GroceryList.Data.Repository;
using GroceryList.Data.Services;

namespace GroceryList.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        public SqlServerContext sqlServerContext { get; private set; }
        public MongoDbService mongoDbService { get; private set; }
        GroceryListRepository _groceryListRepository;
        UserRepository _userRepository;
        ILogger<GroceryListRepository> _logger;

        public UnitOfWork(
            SqlServerContext sqlServerContext,
            MongoDbService mongoDbService,
            ICachingService cache,
            ILogger<GroceryListRepository> logger)
        {
            this.sqlServerContext = sqlServerContext;
            this.mongoDbService = mongoDbService;
            _logger = logger;
            _groceryListRepository = new GroceryListRepository(sqlServerContext, cache, mongoDbService, _logger);
            _userRepository = new UserRepository(sqlServerContext);
		}
        public void Commit()
        {
            sqlServerContext.SaveChangesAsync();
        }

        public GroceryListRepository GroceryListRepository() { return _groceryListRepository; }

        public UserRepository UserRepository() { return _userRepository; }

        public void Roolback(){}
    }
}
