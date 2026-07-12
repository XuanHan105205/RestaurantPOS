using System.Collections.Generic;
using RestaurantPOS.Models;

namespace RestaurantPOS.Repositories
{
    public interface IStockReceiptRepository : IBaseRepository<StockReceipt>
    {
        List<StockReceipt> GetReceiptsByIngredientId(int ingredientId);
    }
}
