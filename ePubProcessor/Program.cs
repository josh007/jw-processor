using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Xml;
using HtmlAgilityPack;
using System.Configuration;

namespace ePubProcessor
{
    class Program
    {
        static string winzip = "";

        static void Main(string[] args)
        {
            // read winzip executable from the config file
            winzip = ConfigurationManager.AppSettings["winzip-executable"];
            string epubDirectory = ConfigurationManager.AppSettings["epub-source-directory"];

            DirectoryInfo dirr = new DirectoryInfo(epubDirectory);
            if (!dirr.Exists)
                return;
                
            // Loop to the directory and process all epub files
            foreach (var file in dirr.GetFiles("*.epub"))
            {
                Process_ePubFiles_ForIPad(file.Name);
            }
        }

        /// <summary>
        /// Process files 
        ///     0. deflate the epub
        ///     1. fix css files
        ///     2. fix xhtml files for citation reference
        ///     3. zip the files and rename as epub
        /// </summary>
        /// <param name="fileName"></param>
        private static void Process_ePubFiles_ForIPad(string fileName)
        {
            // Check the file exists or not
            var file = new FileInfo(fileName);
            if (!file.Exists || !File.Exists(winzip))
                return;   

            // Change the file to zip so we can deflate it
            file.MoveTo(file.Name + ".zip");

            // Get the directory name to be from the file it-self
            string dirName = fileName.Remove(fileName.Length - 5, 5);

            // deflate the epub for processing
            UnzipFile(file, dirName);

            // Process the files in the OEBPS and OEBPS\\CSS folders for both xhtml and css files
            if (!ProcessFilesIn_OEBPSAndCSS_Folders(dirName))
                return;

            // Remove the zipped file so the new zipped file is created for epub file
            file.Delete();

            // re-zip the files
            ZipFile(file, dirName);

            // Rename the zipped files to epub format ....
            file.MoveTo(fileName);
        }

        /// <summary>
        /// Process the files in the OEBPS (xhtml files)
        /// and
        /// Process the files in the OEBPS\\CSS (css files)
        /// </summary>
        /// <param name="dirName"></param>
        /// <returns></returns>
        private static bool ProcessFilesIn_OEBPSAndCSS_Folders(string dirName)
        {
            // Processing files in -----------------OEBPS (xhtml files)----------------------------
            var dir = new DirectoryInfo(dirName + "\\OEBPS");
            if (!dir.Exists)
                return false;

            foreach (FileInfo fi in dir.GetFiles())
            {
                if (fi.Extension.ToLower() == ".xhtml")
                    ProcessCitationReferences_ForIPad(fi.FullName);
            }

            // Processing files in -----------------OEBPS\\CSS (css files)----------------------------
            dir = new DirectoryInfo(dirName + "\\OEBPS\\CSS");
            if (!dir.Exists)
                return false;

            foreach (FileInfo fi in dir.GetFiles())
            {
                if (fi.Extension.ToLower() == ".css")
                    ProcessCSSFixing_ForIPad(fi.FullName);
            }
            return true;
        }

        /// <summary>
        /// Start the winzip process to re-zip the files to make-up z epub
        /// </summary>
        /// <param name="file"></param>
        /// <param name="dirName"></param>
        private static void ZipFile(FileInfo file, string dirName)
        {
            Process winzipProcess = new Process();
            ProcessStartInfo winzipProcessStartInfo = new ProcessStartInfo(winzip, "-min -a -r " + file.Name + " " + dirName + "\\*.*");

            winzipProcessStartInfo.UseShellExecute = false;
            winzipProcessStartInfo.RedirectStandardError = true;
            winzipProcess.StartInfo = winzipProcessStartInfo;
            winzipProcess.Start();

            winzipProcess.WaitForExit();

            // Remove the expanded directory once the files processed
            var dir = new DirectoryInfo(dirName);
            if (dir.Exists)
                dir.Delete(true);
        }

        /// <summary>
        /// Start the winzip process to deflate the epub for processing
        /// </summary>
        /// <param name="file"></param>
        /// <param name="dirName"></param>
        private static void UnzipFile(FileInfo file, string dirName)
        {

            Process winzipProcess = new Process();
            ProcessStartInfo winzipProcessStartInfo = new ProcessStartInfo(winzip, "-min -e -o " + file.Name + " " + dirName);

            winzipProcessStartInfo.UseShellExecute = false;
            winzipProcessStartInfo.RedirectStandardError = true;
            winzipProcess.StartInfo = winzipProcessStartInfo;
            winzipProcess.Start();

            winzipProcess.WaitForExit();
        }

        /// <summary>
        /// This function Fixex the Citation references in the epub file for iPad usage
        /// </summary>
        /// <param name="fileName">string filename</param>
        private static void ProcessCitationReferences_ForIPad(string fileName)
        {
            bool isModified = false;

            HtmlDocument doc = new HtmlDocument();
            doc.Load(fileName);
            doc.OptionWriteEmptyNodes = true;

            // get all the aside elemets from the xhtml file as this is the citation that needs fixing for IPad readers
            var asideNodes = doc.DocumentNode.SelectNodes("//aside");
            if (asideNodes == null)
                return;

            foreach (HtmlNode aside in asideNodes)
            {
                if (aside.HasAttributes && aside.Attributes.FirstOrDefault().Name == "epub:type")
                {
                    foreach (HtmlNode div in aside.ChildNodes.Where(a => a.Name == "div"))
                    {
                        foreach (HtmlNode node in div.ChildNodes)
                        {
                            if (node.Name == "p" && node.InnerHtml.Trim() != "")
                            {
                                node.SetAttributeValue("style", "color:green;font: .85em/1.2em Arial, Helvetica, sans-serif !important;");
                                isModified = true;
                            }
                        }
                    }
                }
            }

            if (!isModified)
                return;

            doc.Save(fileName);

            // The HTMLAgilityPack library while processing the xhtml file removes the crucial ?> closing tag from
            // (<?xml version="1.0" encoding="utf-8" ?>) and changes it to <?xml version="1.0" encoding="utf-8" />
            // the code below revrets it back

            string tmp = "";
            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    while (sr.Peek() >= 0)
                    {
                        char c = (char)sr.Read();
                        if (c == '/' && sr.Peek() == '>')
                        {
                            tmp += "?";
                            break;
                        }
                        tmp += c.ToString();
                    }

                    tmp += sr.ReadToEnd();
                }
            }

            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    writer.Write(tmp);
                }
            }
        }

        /// <summary>
        /// This function fixes the CSS for the whole epub file for iPad usage
        /// </summary>
        /// <param name="fi"></param>
        private static void ProcessCSSFixing_ForIPad(string fileName)
        {
            List<string> lines = new List<string>();
            string tmp = "";
            using (FileStream fs = new FileStream(fileName, FileMode.Open))
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

            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    foreach (var line in lines)
                        writer.WriteLine(line);
                }
            }
        }
    }
}
