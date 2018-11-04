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
using System.IO;
using System.Xml;
using System.Net;
using System.Net.Sockets;


namespace Projeto_DAD
{
    public partial class PuppetMaster : Form
    {
        private Dictionary<string, MyRemoteObject> PCS_Url; //address PCS
        private const int port = 10000; //Port in the PCS

        private string typeOfExecution;
        private string filePath;

        private Dictionary<string, MyRemoteObject> PCS_ClientUrl; //address PCS


        private List<string> commandList = new List<string>();



        //private TcpChannel channel;
        //private MyRemoteObject obj;

        public PuppetMaster()
        {
            InitializeComponent();
            PCS_Url = new Dictionary<string, MyRemoteObject>();
            PCS_ClientUrl = new Dictionary<string, MyRemoteObject>();
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
                foreach (string command in commandList)
                {
                    checkLine(command);
                    Console.WriteLine(command);
                }
            }
            else if (typeOfExecution.Equals("Step"))
            {
                textBox_Browse.Enabled = false;
                foreach (string command in commandList)
                {
                    checkLine(command);
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


        private void button_Run_Click(object sender, EventArgs e)
        {
            //Console.WriteLine("RUN");
            //String command = textBox2.ge
            

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }


        public void checkLine(String line)
        {
            string[] words = line.Split(' ');

            if (words.Length <= 0)
                return;

            string command = words[0];


            string url_toSend;

            MyRemoteObject ipcs;
     
            
                switch (command) { //test command name
                case "Server":  //Start Server
                    string server_id = words[1];
                    string url = words[2];
                    string min_delay = words[3];
                    string max_delay = words[4];

                    url_toSend = "tcp://" + url + ":" + port + "/MyRemoteObjectName";

                    if ( words.Length == 5 )
                    {
                        try
                        {
                            //test URI

                            try
                            {
                                ipcs = PCS_Url[server_id]; //if already registered start
                            }
                            catch (KeyNotFoundException e)
                            {
                                
                                Console.WriteLine("Connecting to {0}", url);

                                TcpChannel channel = new TcpChannel();
                                ChannelServices.RegisterChannel(channel, true);

                                ipcs = (MyRemoteObject)Activator.GetObject(typeof(MyRemoteObject), url_toSend);
                                //pcs.Print("Hello there :)");
                                PCS_Url.Add(url, ipcs);
                                
                            }

                            ipcs.StartServer(server_id, min_delay, max_delay);
                           
                        }
                        catch (UriFormatException e)
                        {
                            Console.WriteLine("Invalid PCS_URL: {0}", e);
                        }

                    }
                    break;
                case "Client":  //Start Server
                    string client_id = words[1];
                    string url_client = words[2];
                    string scriptFile = words[3];

                    url_toSend = "tcp://" + url_client + ":" + port + "/MyRemoteObjectName";

                    if (words.Length == 4)
                    {
                        try
                        {
                            //testc URI
                            try
                            {
                                ipcs = PCS_ClientUrl[client_id]; //if already registered start
                            }
                            catch (KeyNotFoundException e)
                            {

                                Console.WriteLine("Connecting to {0}", url_client);

                                TcpChannel channel = new TcpChannel();
                                ChannelServices.RegisterChannel(channel, true);

                                ipcs = (MyRemoteObject)Activator.GetObject(typeof(MyRemoteObject), url_toSend);
                                //pcs.Print("Hello there :)");
                                PCS_ClientUrl.Add(url_client, ipcs);

                            }

                            ipcs.StartClient(client_id, url_client, scriptFile);

                        }
                        catch (UriFormatException e)
                        {
                            Console.WriteLine("Invalid PCS_URL: {0}", e);
                        }
                    }
                    break;
                
                default:
                    Console.WriteLine("Invalid Command");
                    break;
            }
        }// end_checkline

        

    }
}
