using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.util;
using Matrix = org.apache.pdfbox.util.Matrix;

namespace PDFReader
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            ImportDataToSQLite importer = new ImportDataToSQLite();
            rtxtMain.Text = importer.Import(bookName:"joshua", fileName: @"E:\share\joshua.pdf");

        }

        private void btnMap_Click(object sender, EventArgs e)
        {
            ImportDataToSQLite importer = new ImportDataToSQLite();
            var name = "joshua";
            var result = importer.MapBookVerses(name);

            rtxtMain.AppendText("The book of " + name + Environment.NewLine + Environment.NewLine);
            rtxtMain.AppendText("# of chapters " + result.Count + Environment.NewLine);

            foreach (KeyValuePair<int, int> keyValuePair in result)
            {
                rtxtMain.AppendText(Environment.NewLine + "Chapter " + keyValuePair.Key + Environment.NewLine);
                rtxtMain.AppendText("        ");

                for (int i = 1; i <= keyValuePair.Value; i++)
                    rtxtMain.AppendText(i + " ");
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            btnMap_Click(this,new EventArgs());
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
