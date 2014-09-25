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
            SQLiteManager sqlMgr = new SQLiteManager("Data Source=joshdb.sqlite;Version=3;foreign keys=true;");

            //create the repository
            //sqlMgr.CreateDataBase();

            //create tables
            sqlMgr.CreateTableStructures();

            //populate the data
            sqlMgr.PopulateTestData();

            //sql = "SELECT * FROM highscores WHERE score > 0";
            //com.CommandText = sql;
            //SQLiteDataReader reader = com.ExecuteReader();

            //while (reader.Read())
            //{
            //    Console.WriteLine("ID = {0}, Name = {1}, Score = {2}", reader[0], reader[1], reader[2]);
            //}
            //reader.Close();
            //sql = "DROP TABLE highscores";
            //com.CommandText = sql;
            //com.ExecuteNonQuery();

            Console.Read();
        }
    }
}
