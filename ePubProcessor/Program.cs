using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Xml;

namespace ePubProcessor
{
    class Program
    {
        static bool isExited = false;
        const string aside = "<aside epub:type=footnote>";
        private static string test()
        {
            StringBuilder output = new StringBuilder();

            String xmlString =
                @"<bookstore>
                    <book genre='autobiography' publicationdate='1981-03-22' ISBN='1-861003-11-0'>
                        <title>The Autobiography of Benjamin Franklin</title>
                        <author>
                            <first-name>Benjamin</first-name>
                            <last-name>Franklin</last-name>
                        </author>
                        <price>8.99</price>
                    </book>
                </bookstore>";

            // Create an XmlReader
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            {
                reader.ReadToFollowing("book");
                reader.MoveToFirstAttribute();
                string genre = reader.Value;
                output.AppendLine("The genre value: " + genre);

                reader.ReadToFollowing("title");
                output.AppendLine("Content of the title element: " + reader.ReadElementContentAsString());
            }

            return output.ToString();
        }
        static void Main(string[] args)
        {
            test();
            return;
            string fileName = @"w_AM_20140615.epub";
            //string fileName = @"w_AM_20140715.epub";
            //string fileName = @"w_AM_20140915.epub";
            //string fileName = @"w_AM_20141015.epub";
            //string fileName = @"w_AM_20141115.epub";


            string winzip = @"c:\Program Files\WinZip\Winzip32.exe";
            string dirName = fileName.Remove(fileName.Length - 5, 5);

            FileInfo file = new FileInfo(fileName);

            if (!file.Exists || !File.Exists(winzip))
                return;

            file.MoveTo(file.Name + ".zip");

            var process = Process.Start(winzip, "-min -e -o " + file.Name + " " + dirName);
            process.Exited += process_Exited;

            do
            {
                Thread.Sleep(1000);
            } while (isExited);

            DirectoryInfo dir = new DirectoryInfo(dirName + "\\OEBPS");

            if (!dir.Exists)
                return;

            Console.WriteLine("---------Processing files in OEBPS");
            foreach (FileInfo fi in dir.GetFiles())
            {
                string lines = "";
                string tmp = "";
                using (FileStream fs = new FileStream(fi.FullName, FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {

                        while (sr.Peek() >= 0)
                        {
                            tmp = sr.ReadLine();
                            if (tmp.Contains(aside))
                                tmp = ProcessAside(tmp);

                            lines += tmp;
                        }
                    }
                }

                using (FileStream fs = new FileStream(fi.FullName, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        writer.Write(lines);
                    }
                }

                Console.WriteLine(@"Accessing {0}", fi.Name);
            }
            //Console.ReadLine();

            dir = new DirectoryInfo(dirName + "\\OEBPS\\CSS");

            if (!dir.Exists)
                return;

            Console.WriteLine("---------Processing files in OEBPS\\CSS");
            foreach (FileInfo fi in dir.GetFiles())
            {

                List<string> lines = new List<string>();
                string tmp = "";
                using (FileStream fs = new FileStream(fi.FullName, FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {

                        while (sr.Peek() >= 0)
                        {

                            tmp = sr.ReadLine();
                            if (tmp.Contains("\"Times New Roman\""))
                                tmp = "\tfont-family: Arial, Helvetica, sans-serif;";
                            lines.Add(tmp);
                        }
                    }
                }

                using (FileStream fs = new FileStream(fi.FullName, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        foreach (var line in lines)
                            writer.WriteLine(line);
                    }
                }

                Console.WriteLine(@"Accessing {0}", fi.Name);
            }

            file.Delete();
            process = Process.Start(winzip, "-min -a -r " + file.Name + " " + dirName + "\\*.*");
            process.Exited += process_Exited;

            do
            {
                Thread.Sleep(1000);
            } while (isExited);

            dir = new DirectoryInfo(dirName);

            if (dir.Exists)
                dir.Delete(true);

            file.MoveTo(fileName);
            Console.Read();
        }

        private static string ProcessAside(string tmp)
        {
            int start = tmp.IndexOf(aside, System.StringComparison.Ordinal);
            
            for(int i = start; i<tmp.Length; i++)
            {
                //if(tmp[i]==)
            }

            return null;
        }

        static void process_Exited(object sender, EventArgs e)
        {
            isExited = true;
        }
    }
}
