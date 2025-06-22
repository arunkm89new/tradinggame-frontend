using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Maui.Storage;
using SQLite;
using TradingGame.Models;

namespace TradingGame.Services
{
    public class TradeService:BaseSqlLiteService
    {
        
        public TradeService()
        {
            _db.CreateTableAsync<TradeModel>().Wait();
        }

        public Task<int> SaveTradeAsync(TradeModel trade)
        {
            if (trade.Id != 0)
                return _db.UpdateAsync(trade);
            return _db.InsertAsync(trade);
        }

        public Task<List<TradeModel>> GetOpenTradesAsync() =>
            _db.Table<TradeModel>().Where(t => t.IsOpen).ToListAsync();

        public Task<List<TradeModel>> GetAllTradesAsync() =>
            _db.Table<TradeModel>().ToListAsync();

        public Task<int> CloseTradeAsync(TradeModel trade, decimal exitPrice)
        {
            trade.IsOpen = false;
            trade.ExitPrice = exitPrice;
            trade.CloseTime = DateTime.UtcNow;
            return _db.UpdateAsync(trade);
        }
        public static void DeleteDatabase()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, DatabaseFileName);
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
        }
    }
}
