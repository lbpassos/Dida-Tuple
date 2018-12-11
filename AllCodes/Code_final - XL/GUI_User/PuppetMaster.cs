using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.IO;
using System.Net;
using System.Net.Sockets;


namespace Projeto_DAD
{
    public partial class PuppetMaster : Form
    {
        //private Dictionary<string, IPCSServices> PCS_Url; //address PCS
        //private Dictionary<string, IPCSServices> PCS_ClientUrl; //address PCS

        //private const int port_PCS = 1000; //Port in the PCS
        private const string PCS_Local_address= "tcp://localhost:10000/MyRemoteObjectName";

        private string filePath;

        
        private bool button_nextStep_WasClicked = false;

        private List<string> commandList = new List<string>();



        
        private TcpChannel channel;
        private IPCSServices ipcs;


        public PuppetMaster()
        {
            InitializeComponent();
            //PCS_Url = new Dictionary<string, IPCSServices>();
            //PCS_ClientUrl = new Dictionary<string, IPCSServices>();
            channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel, true);
            ipcs = (IPCSServices)Activator.GetObject(typeof(IPCSServices), PCS_Local_address);

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
                //XmlDocument xmlDoc = new XmlDocument();
                //xmlDoc.Load(filePath);
                //typeOfExecution = xmlDoc.DocumentElement.SelectSingleNode("type").InnerText;  //Verifica qual o tipo de execução que o PM vai fazer, Sequence ou Step by Step
                //foreach (XmlNode node in xmlDoc.DocumentElement.SelectNodes("command"))
                //{
                //    commandList.Add(node.InnerText); //Adiciona os comandos à lista de comandos
                //}

                //Scanner for .txt file
                const Int32 BufferSize = 128;
                using (var fileStream = File.OpenRead(filePath))
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                {
                    String line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        commandList.Add(line);
                    }
  
                }

                button_Sequence.Enabled = true;
                button_Step.Enabled = true;
            }
        }

        private void button_Sequence_Click(object sender, EventArgs e)
        {
            button_Step.Enabled = false;
            foreach (string command in commandList)
            {
                Console.WriteLine(command);
                checkLine(command);
            }
        }

        private void button_Step_Click(object sender, EventArgs e)
        {
            button_Sequence.Enabled = false;
            button_NextStep.Enabled = true;

            new Thread(() => _wasCliked(button_Step,button_NextStep)) { IsBackground = true }.Start();

        }

        private void button_NextStep_Click(object sender, EventArgs e)
        {
            button_nextStep_WasClicked = true;
        }

        private void _wasCliked(Button buttonStep,Button buttonNextStep)
        {
            foreach (string command in commandList)
            {
                while (button_nextStep_WasClicked == false) ;
                Console.WriteLine(command);
                checkLine(command);
                button_nextStep_WasClicked = false;
            }
        }


        private void button_Run_Click(object sender, EventArgs e)
        {
            //Console.WriteLine("RUN");
            string command = textBox2.Text;
            checkLine(command);

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
                 
            switch (command) { //test command name
                case "Server":  //Start Server
                    string server_id = words[1];
                    string url = words[2];
                    string min_delay = words[3];
                    string max_delay = words[4];

                    if ( words.Length == 5 )
                    {

                        try
                        {
                            ipcs.StartServer(server_id, url, min_delay, max_delay);
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine("PCS is DOWN " + e);
                        }
                    }                       
                    break;
                case "Client":  //Start Client

                    string client_id = words[1];
                    string url_client = words[2];
                    string scriptFile;
                    try
                    {
                        scriptFile = words[3];
                    }
                    catch
                    {
                        scriptFile = null;
                    }
                   
                    if (words.Length == 4)
                    {
                        try
                        {
                            ipcs.StartClient(client_id, url_client, scriptFile);
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine("PCS is DOWN");
                        }
                    }
                    break;
                case "Status":
                    if (words.Length == 1)
                    {
                        try
                        {
                            ipcs.Status();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("PCS is DOWN");
                        }
                    }
                    break;
                case "Freeze":
                    string proc = words[1];

                    if (words.Length == 2)
                    {
                        try
                        {
                            ipcs.Freeze(proc);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("PCS is DOWN");
                        }
                    }
                    break;
                case "Unfreeze":
                    string proc_ = words[1];

                    if (words.Length == 2)
                    {
                        try
                        {
                            ipcs.Unfreeze(proc_);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("PCS is DOWN");
                        }
                    }
                    break;
                case "Crash":
                    string _proc = words[1];

                    if (words.Length == 2)
                    {
                        try
                        {
                            ipcs.Crash(_proc);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("PCS is DOWN");
                        }
                    }
                    break;

                default:
                    Console.WriteLine("Invalid Command");
                    break;
            }
        }// end_checkline

        private void GUI_Client_Load(object sender, EventArgs e)
        {

        }

    }
}
