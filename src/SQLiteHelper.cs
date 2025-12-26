using SQLite;
using System.Collections.Concurrent;

namespace MetaFrm.SQLite
{
    /// <summary>
    /// SQLite를 처리하기 위한 클래스입니다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SQLiteHelper<T> : ISQLiteHelper<T> where T : new()
    {
        private static readonly ConcurrentDictionary<string, SQLiteAsyncConnection> _connections = [];
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _initLocks = new();
        private static readonly ConcurrentDictionary<string, bool> _initializedTables = new();

        private static async Task<SQLiteAsyncConnection> GetConnectionAsync()
        {
            string key = typeof(T).FullName!;

            var connection = _connections.GetOrAdd(key, k =>
            {
                string dbPath = Path.Combine(Factory.AppDataDirectory ?? "", $"{k.Replace(".", "")}.db3");

                return new SQLiteAsyncConnection(dbPath);
            });

            if (_initializedTables.ContainsKey(key))
                return connection;

            // 테이블 생성은 최초 1회만 보장
            var initLock = _initLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
            await initLock.WaitAsync();

            try
            {
                if (!_initializedTables.ContainsKey(key))
                {
                    await connection.CreateTableAsync<T>();
                    _initializedTables[key] = true;
                }
            }
            finally
            {
                initLock.Release();
                _initLocks.TryRemove(key, out _);
            }

            return connection;
        }

        /// <summary>
        /// Add
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<int> AddAsync(T value)
        {
            var connection = await GetConnectionAsync();
            return await connection.InsertAsync(value);
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<int> DeleteAsync(T value)
        {
            var connection = await GetConnectionAsync();
            return await connection.DeleteAsync(value);
        }

        /// <summary>
        /// GetList
        /// </summary>
        /// <returns></returns>
        public async Task<List<T>> GetListAsync()
        {
            var connection = await GetConnectionAsync();
            return await connection.Table<T>().ToListAsync();
        }

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<int> UpdateAsync(T value)
        {
            var connection = await GetConnectionAsync();
            return await connection.UpdateAsync(value);
        }

        /// <summary>
        /// 동기 Transaction (권장)
        /// </summary>
        public async Task RunInTransactionAsync(Action<ISQLiteConnection> action)
        {
            var asyncConn = await GetConnectionAsync();
            await asyncConn.RunInTransactionAsync(action);
        }
    }
}