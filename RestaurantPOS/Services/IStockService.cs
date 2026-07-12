using System.Collections.Generic;
using RestaurantPOS.Models;

namespace RestaurantPOS.Services
{
    public interface IStockService
    {
        List<StockReceipt> GetAllReceipts();
        List<StockReceipt> GetReceiptsByIngredientId(int ingredientId);
        bool AddStockReceipt(StockReceipt receipt);
    }
}
