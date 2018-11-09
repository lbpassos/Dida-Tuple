using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common_types;

namespace Server
{
    class ServerService : MarshalByRefObject, IServerServices
    {

        //public static List<string> lista = new List<string>();

        private static bool Root = false;

        private static Image image = new Image();

        
        public static void setRoot(bool value)
        {
            Root = value;
        }

        public static void add(String s)
        {
            image.Add(s);
        }
        public static string read(int i)
        {
            return image.Read(i);
        }
        public static void take(string i)
        {
            image.Take(i);
        }
        public static void setImage(Object img)
        {
            image = (Image)img;
        }
        public static string getImageRepresentation()
        {
            return image.ToString();
        }


        //Implement Interface IServerToServer
        public void Ping()
        {
            return;
        }        
        public bool isRoot()
        {
            return Root;
        }
        public Object getImage()
        {
            return image;
        }


    }
}
