using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace POGApp
{
    public class Program
    {
        private const string NAME = "POGService";
        private const string IP = "localhost";
        private const int PORT = 9001;

        public static void Main(string[] args)
        {
            //Run(new Program());

            var host = new ServiceHost(typeof(POGService));
            try
            {
                host.Open();
                foreach (Uri uri in host.BaseAddresses)
                {
                    Console.WriteLine(" - Host opened at " + uri.AbsoluteUri);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error opening host: " + ex);
            }
            finally
            {
                if (host.State == CommunicationState.Opened)
                {
                    Console.WriteLine("Opened");
                    //labelStatus.Content = "Opened";
                    //buttonStop.IsEnabled = true;
                }
            }
            Console.ReadLine();
        }
    }
}
