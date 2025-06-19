using System;
using SQLite;

namespace TradingGame.Models
{
    public class UserModel
    {
        [PrimaryKey]
        public int Id { get; set; } = 1; // Single user for this app
        public decimal CashBalance { get; set; } = 10000; // Starting balance of $10,000
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}