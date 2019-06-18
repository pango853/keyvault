using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data.Common;

namespace KeyVault
{
    internal static class DBHelper
    {
        //private static readonly string DB_SOURCE = "keyvault.db";

        public static void InitDB()
        {
            SQLiteConnection conn = new SQLiteConnection(String.Format("Data Source={0};Version=3;", App.DB_PATH));
            conn.Open();

            using (DbCommand command = conn.CreateCommand())
            {
                command.CommandText = "CREATE TABLE IF NOT EXISTS services(" +
                    "id INTEGER PRIMARY KEY," +
                    "name VARCHAR(127) NOT NULL UNIQUE," +
                    "url VARCHAR(255) NOT NULL UNIQUE," +
                    "created TIMESTAMP DEFAULT(datetime(CURRENT_TIMESTAMP,'localtime')));";
                command.ExecuteNonQuery();
            }
            using (DbCommand command = conn.CreateCommand())
            {
                command.CommandText = "CREATE TABLE IF NOT EXISTS keypairs(" +
                    "id INTEGER PRIMARY KEY," +
                    "service INTEGER NOT NULL," +
                    "user VARCHAR(128) NOT NULL," +
                    "secret VARCHAR(4096) NOT NULL," +
                    "note TEXT DEFAULT NULL," +
                    "created TIMESTAMP DEFAULT(datetime(CURRENT_TIMESTAMP,'localtime'))," +
                    "FOREIGN KEY(service) REFERENCES services(id));";
                command.ExecuteNonQuery();
            }
            using (DbCommand command = conn.CreateCommand())
            {
                command.CommandText = "CREATE UNIQUE INDEX IF NOT EXISTS keypairs_service_user_idx ON keypairs (service, user);";
                command.ExecuteNonQuery();
            }
            conn.Close();
        }

        public static void AddRecord(string name, string url, string user, string pswd, string note)
        {
            SQLiteConnection conn = new SQLiteConnection(String.Format("Data Source={0};Version=3;", App.DB_PATH));
            conn.Open();

            using (DbCommand command = conn.CreateCommand())
            {
                command.CommandText = "INSERT INTO services (name, url) VALUES(@Name, @URL)";
                command.Parameters.Add(new SQLiteParameter("Name", name));
                command.Parameters.Add(new SQLiteParameter("URL", url));

                try
                {
                    command.ExecuteNonQuery();
                }
                catch (SQLiteException e)
                {
                    throw new DBInsertServiceException(e.Message);
                }
            }
            using (DbCommand command = conn.CreateCommand())
            {
                string secret = SecretHelper.Encrypt(pswd);
                if (String.IsNullOrWhiteSpace(note)) note = null;

                command.CommandText = "INSERT INTO keypairs (service, user, secret, note) " +
                    "SELECT id, @Username, @Secret, @Note FROM services WHERE name LIKE @Name LIMIT 1";
                command.Parameters.Add(new SQLiteParameter("Name", name));
                command.Parameters.Add(new SQLiteParameter("Username", user));
                command.Parameters.Add(new SQLiteParameter("Secret", secret));
                command.Parameters.Add(new SQLiteParameter("Note", note));

                try
                {
                    command.ExecuteNonQuery();
                }
                catch (SQLiteException e)
                {
                    throw new DBInsertServiceException(e.Message);
                }
            }
            conn.Close();
        }

        public static int Count()
        {
            int total = 0;
            var connectionSb = new SQLiteConnectionStringBuilder { DataSource = App.DB_PATH, Version = 3 };
            using (var conn = new SQLiteConnection(connectionSb.ToString()))
            {
                conn.Open();

                using (DbCommand command = conn.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(k.id) FROM services AS s LEFT JOIN keypairs as k ON s.id = k.service";
                    total = Convert.ToInt32(command.ExecuteScalar());
                }

                conn.Close();
            }
            return total;
        }

        public static int Count(string pattern)
        {
            if (String.IsNullOrWhiteSpace(pattern)) return Count();

            int count = 0;
            var connectionSb = new SQLiteConnectionStringBuilder { DataSource = App.DB_PATH, Version = 3 };
            using (var conn = new SQLiteConnection(connectionSb.ToString()))
            {
                conn.Open();

                using (DbCommand command = conn.CreateCommand())
                {
                    command.CommandText = "SELECT s.id, s.name, s.url, k.user FROM services AS s " +
                        "LEFT JOIN keypairs as k ON s.id = k.service " +
                        "WHERE s.name LIKE '%' || @pattern || '%' OR s.url LIKE '%' || @pattern || '%'";
                    command.Parameters.Add(new SQLiteParameter("pattern", pattern));

                    count = Convert.ToInt32(command.ExecuteScalar());
                }
                conn.Close();
            }

            return count;
        }

        public static List<string[]> Find(string pattern)
        {
            var tuples = new List<string[]>();
            if ( pattern.Length == 0 || pattern == null) return tuples;

            var connectionSb = new SQLiteConnectionStringBuilder { DataSource = App.DB_PATH, Version = 3 };
            using (var conn = new SQLiteConnection(connectionSb.ToString()))
            {
                conn.Open();

                using (DbCommand command = conn.CreateCommand())
                {
                    command.CommandText = "SELECT s.name, s.url, k.user, k.secret FROM services AS s " +
                        "LEFT JOIN keypairs as k ON s.id = k.service " +
                        "WHERE s.name LIKE '%' || @pattern || '%' OR s.url LIKE '%' || @pattern || '%' " +
                        "ORDER BY s.name, s.url, k.created DESC LIMIT 10";
                    command.Parameters.Add(new SQLiteParameter("pattern", pattern));
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read() == true)
                        {
                            string[] record = new string[4];
                            record[0] = reader["name"].ToString();
                            record[1] = reader["url"].ToString();
                            record[2] = reader["user"].ToString();
                            record[3] = reader["secret"].ToString();
                            tuples.Add(record);
                        }
                        reader.Close();
                    }
                }
                conn.Close();
            }

            return tuples;
        }
    }
}
