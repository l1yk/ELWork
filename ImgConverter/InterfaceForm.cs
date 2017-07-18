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
    public partial class InterfaceForm : Form
    {
        public InterfaceForm()
        {
            InitializeComponent();
        }

        private void InterfaceForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Link;
        }

        private void InterfaceForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 1) MessageBox.Show("不支援多檔拖放，只會取第一個檔案");
            txtSourcePath.Text = files[0];
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Multiselect = false;
            if (op.ShowDialog() == DialogResult.OK)
            {
                txtSourcePath.Text = op.FileName;
            }
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtSourcePath.Text))
            {
                OpenFileDialog op = new OpenFileDialog();
                op.Multiselect = false;
                if (op.ShowDialog() == DialogResult.OK)
                {
                    txtSourcePath.Text = op.FileName;
                }
            }

            string tmpFilePath = txtSourcePath.Text;

            try
            {
                File.WriteAllBytes(tmpFilePath + ".vfd", ImgConvert.DecodeIMG(File.ReadAllBytes(tmpFilePath)));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
