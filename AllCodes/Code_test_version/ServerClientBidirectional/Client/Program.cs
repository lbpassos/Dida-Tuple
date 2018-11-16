using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerClientBidirectional
{
    class Program
    {
        static void Main(string[] args)
        {
            int port = Int32.Parse(args[0]);

            Client client = new Client(port);
            Console.WriteLine("Cliente no porto: " + port);

            string input_str;
            while (true)
            {
                Console.Write("Comando: ");
                input_str = Console.ReadLine();

                client.SendMessage(input_str);

                while (client.IsMessageReceived() == false) ;
                Console.WriteLine("Mensagem Recebida: " + client.GetMessage());
            }

        }
    }
}
