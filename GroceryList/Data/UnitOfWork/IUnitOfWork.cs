using GroceryList.Data.Repository;

namespace GroceryList.Data.UnitOfWork
{
    public interface IUnitOfWork
    {
        public GroceryListRepository GroceryListRepository();
        public UserRepository UserRepository();
        public void Commit();
        public void Roolback();
    }
}
