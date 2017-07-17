using ELWork;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImgConverter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string tmpPath = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
            txtPath.Text = tmpPath;
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            byte[] tmpIMG = File.ReadAllBytes(txtPath.Text);
            File.WriteAllBytes(txtPath.Text + ".vfd", ImgConvert.DecodeIMG(tmpIMG));
        }
    }
}
