using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
namespace ePubProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileName = @"w_AM_20141015.epub";
            FileInfo file = new FileInfo(fileName);

            if (!file.Exists)
                return;

            file.MoveTo(file.Name + ".zip");

            Process.Start("winzip.exe -x " + file.Name);

            DirectoryInfo dir = new DirectoryInfo(file.Name + "\\OEBPS");

            if (!dir.Exists)
                return;

            foreach (FileInfo fi in dir.GetFiles())
            {
                Console.WriteLine(@"Accessing {0}\{1}", fi.FullName, fi.Name);
            }

            dir = new DirectoryInfo(file.Name + "\\OEBPS\\CSS");

            if (!dir.Exists)
                return;

            foreach (FileInfo fi in dir.GetFiles())
            {
                Console.WriteLine(@"Accessing {0}\{1}", fi.FullName, fi.Name);
            }

            Console.Read();
        }
    }
}
