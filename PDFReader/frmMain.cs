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
            rtxtMain.Text = importer.Import("joshua", @"e:\share\joshua.pdf");

        }
    }
}
