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
using System.Xml;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Projeto_DAD
{
    public partial class PuppetMaster : Form
    {
        private string typeOfExecution;
        private string filePath;
        private List<string> commandList = new List<string>();

        public PuppetMaster()
        {
            InitializeComponent();

            int port = 10001;
            TcpClient client = new TcpClient("localhost", port);
            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream);
            StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };
        }

        private void button_Browse_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.IO.StreamReader sr = new System.IO.StreamReader(openFileDialog1.FileName);
                filePath = openFileDialog1.FileName;
                textBox_Browse.AppendText(filePath);
                sr.Close();

                //Scanner for XML file
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(filePath);
                typeOfExecution = xmlDoc.DocumentElement.SelectSingleNode("type").InnerText;  //Verifica qual o tipo de execução que o PM vai fazer, Sequence ou Step by Step
                foreach (XmlNode node in xmlDoc.DocumentElement.SelectNodes("command"))
                {
                    commandList.Add(node.InnerText); //Adiciona os comandos à lista de comandos
                }

                button_Send.Enabled = true;
            }
        }

        private void button_Send_Click(object sender, EventArgs e)
        {
            if (typeOfExecution.Equals("Sequence"))
            {
                textBox_Browse.Enabled = false;
                foreach(string command in commandList)
                {
                    //TODO - execute the comands
                    Console.WriteLine(command);
                }
            }
            else if (typeOfExecution.Equals("Step"))
            {
                textBox_Browse.Enabled = false;
                foreach (string command in commandList)
                {
                    //TODO - execute the comands
                    Console.WriteLine(command);
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("The type of execution is not acceptable, please choose between \"Sequence\" and \"Step\"");
            }
        }

        private void GUI_Client_Load(object sender, EventArgs e)
        {

        }
    }
}
