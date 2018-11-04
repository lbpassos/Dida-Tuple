using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;


namespace Projeto_DAD
{
    public partial class PuppetMaster : Form
    {

        private string filePath;

        public PuppetMaster()
        {
            InitializeComponent();
        }

        private void button_Browse_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.IO.StreamReader sr = new System.IO.StreamReader(openFileDialog1.FileName);
                filePath = openFileDialog1.FileName;
                textBox_Browse.AppendText(filePath);
                sr.Close();
                button_Send.Enabled = true;
            }
        }

        private void button_Send_Click(object sender, EventArgs e)
        {
            textBox_Browse.Enabled = false;
        }

        private void GUI_Client_Load(object sender, EventArgs e)
        {

        }
    }
}
