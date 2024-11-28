using SQLite;

namespace MetaFrm.SQLite
{
    /// <summary>
    /// SQLite를 처리하기 위한 클래스입니다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SQLiteHelper<T> : ISQLiteHelper<T> where T : new()
    {
        private static readonly Dictionary<string, SQLiteAsyncConnection> connectionDictionary = [];


        private static async Task ConnectionDB(string fullName)
        {
            if (!connectionDictionary.TryGetValue(fullName, out _))
            {
                string dbPath = Path.Combine(Factory.AppDataDirectory ?? "", $"{fullName.Replace(".", "")}.db3");

                connectionDictionary.Add(fullName, new SQLiteAsyncConnection(dbPath));

                // Create Table
                if (connectionDictionary.TryGetValue(fullName, out SQLiteAsyncConnection? connection))
                    await connection.CreateTableAsync<T>();
            }
        }

        /// <summary>
        /// Add
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<int> AddAsync(T value)
        {
            string fullName = $"{typeof(T).FullName}";

            await ConnectionDB(fullName);

            if (connectionDictionary.TryGetValue(fullName, out SQLiteAsyncConnection? connection))
                return await connection.InsertAsync(value);

            return 0;
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<int> DeleteAsync(T value)
        {
            string fullName = $"{typeof(T).FullName}";

            await ConnectionDB(fullName);

            if (connectionDictionary.TryGetValue(fullName, out SQLiteAsyncConnection? connection))
                return await connection.DeleteAsync(value);

            return 0;
        }

        /// <summary>
        /// GetList
        /// </summary>
        /// <returns></returns>
        public async Task<List<T>> GetListAsync()
        {
            string fullName = $"{typeof(T).FullName}";

            await ConnectionDB(fullName);

            if (connectionDictionary.TryGetValue(fullName, out SQLiteAsyncConnection? connection))
            {
                var table = connection.Table<T>();

                if (table != null)
                    return await table.ToListAsync();
            }

            return [];
        }

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<int> UpdateAsync(T value)
        {
            string fullName = $"{typeof(T).FullName}";

            await ConnectionDB(fullName);

            if (connectionDictionary.TryGetValue(fullName, out SQLiteAsyncConnection? connection))
                return await connection.UpdateAsync(value);

            return 0;
        }
    }
}