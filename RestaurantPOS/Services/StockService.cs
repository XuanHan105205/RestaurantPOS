using System;
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

        public StockService(IStockReceiptRepository stockReceiptRepository)
        {
            _stockReceiptRepository = stockReceiptRepository;
        }

        public List<StockReceipt> GetAllReceipts()
        {
            return _stockReceiptRepository.GetAll() ?? new List<StockReceipt>();
        }

        public List<StockReceipt> GetReceiptsByIngredientId(int ingredientId)
        {
            if (ingredientId <= 0) return new List<StockReceipt>();
            return _stockReceiptRepository.GetReceiptsByIngredientId(ingredientId) ?? new List<StockReceipt>();
        }

        public bool AddStockReceipt(StockReceipt receipt)
        {
            if (receipt == null) return false;
            if (receipt.IngredientId <= 0) return false;
            if (receipt.Quantity <= 0) return false;
            if (receipt.UnitCost.HasValue && receipt.UnitCost.Value < 0) return false;

            receipt.ReceivedAt = DateTime.Now;
            return _stockReceiptRepository.Add(receipt);
        }
    }
}
