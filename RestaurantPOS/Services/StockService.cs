using System.Collections.Generic;
using RestaurantPOS.Models;
using RestaurantPOS.Repositories;

namespace RestaurantPOS.Services
{
    public class StockService : IStockService
    {
        private readonly IStockReceiptRepository _stockReceiptRepository;

        public StockService()
        {
            _stockReceiptRepository = new StockReceiptRepository();
        }

        public List<StockReceipt> GetAllReceipts()
        {
            return _stockReceiptRepository.GetAll();
        }

        public List<StockReceipt> GetReceiptsByIngredientId(int ingredientId)
        {
            return _stockReceiptRepository.GetReceiptsByIngredientId(ingredientId);
        }

        public bool AddStockReceipt(StockReceipt receipt)
        {
            return _stockReceiptRepository.Add(receipt);
        }
    }
}
