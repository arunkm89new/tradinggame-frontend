using System;
using System.Threading.Tasks;
using SQLite;
using TradingGame.Models;

namespace TradingGame.Services
{
    public class UserService : BaseSqlLiteService
    {
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public async Task InitializeAsync()
        {
            await _db.CreateTableAsync<UserModel>();
            await InitializeUserAsync();
        }
        
        private async Task InitializeUserAsync()
        {
            var user = await GetUserAsync();
            if (user == null)
            {
                // Create default user with initial balance
                user = new UserModel();
                await _db.InsertAsync(user);
            }
        }

        public async Task<UserModel> GetUserAsync()
        {
            return await _db.Table<UserModel>().FirstOrDefaultAsync();
        }
        
        public async Task<decimal> GetCashBalanceAsync()
        {
            var user = await GetUserAsync();
            return user?.CashBalance ?? 10000m;
        }
        
        public async Task UpdateCashBalanceAsync(decimal amount)
        {
            await _semaphore.WaitAsync();
            try
            {
                var user = await GetUserAsync();
                if (user != null)
                {
                    user.CashBalance += amount;
                    user.LastUpdated = DateTime.UtcNow;
                    await _db.UpdateAsync(user);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}