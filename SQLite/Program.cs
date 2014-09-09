using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace SQLite
{
    class Program
    {
        static void Main(string[] args)
        {
            //createDB();
            SQLiteConnection con = new SQLiteConnection("Data Source=joshdb.sqlite;Version=3;");
             
            con.Open();

            string sql;
            SQLiteCommand com = new SQLiteCommand(con);
            JWProcessor
            CreateTables(com);

            //PopulateData(com);

            sql = "SELECT * FROM highscores WHERE score > 0";
            com.CommandText = sql;
            SQLiteDataReader reader = com.ExecuteReader();

            while (reader.Read())
            {
                Console.WriteLine("ID = {0}, Name = {1}, Score = {2}", reader[0], reader[1], reader[2]);
            }
            reader.Close();
            //sql = "DROP TABLE highscores";
            //com.CommandText = sql;
            //com.ExecuteNonQuery();

            Console.Read();
        }

        private static void CreateTables(SQLiteCommand com)
        {
            string sql = "CREATE TABLE books(id INT PRIMARY KEY, name VARCHAR(50))";
            com.CommandText = sql;
            com.ExecuteNonQuery();

            sql = "CREATE TABLE chapters(id INT PRIMARY KEY, book_id INT, chapter INT)";
            com.CommandText = sql;
            com.ExecuteNonQuery();

            sql = "CREATE TABLE verses(id INT PRIMARY KEY, chapter_id INT)";
            com.CommandText = sql;
            com.ExecuteNonQuery();

            sql = "CREATE TABLE verses_details(id INT PRIMARY KEY, sequence INT, verse_id INT, font_name VARCHAR(50))";
            com.CommandText = sql;
            com.ExecuteNonQuery();


        }

        private static void PopulateData(SQLiteCommand com)
        {
            string sql = "INSERT INTO books(id, name)VALUES(6,'joshua')";
            com.CommandText = sql;
            com.ExecuteNonQuery();

            sql = "INSERT INTO chapters(id, book_id, chapter)VALUES(1,6,1)";
            com.CommandText = sql;
            com.ExecuteNonQuery();

        }

        private static void createDB()
        {
            SQLiteConnection.CreateFile("joshdb.sqlite");
        }
    }
}
