using WaitTimeTesting.Models;

namespace WaitTimeTesting.Services
{
    public interface IOrderStorage
    {
        void StoreCompleted(Order order);
    }
}
