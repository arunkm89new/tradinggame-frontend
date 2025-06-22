using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingGame.Services
{
    public class BaseSqlLiteService
    {
        public readonly SQLiteAsyncConnection _db;
        public static readonly string DatabaseFileName = "tradinggame.db3";
        public static string DatabasePath => Path.Combine(FileSystem.AppDataDirectory, DatabaseFileName);
        public BaseSqlLiteService()
        {
            _db = new SQLiteAsyncConnection(DatabasePath);

        }
    }
}
