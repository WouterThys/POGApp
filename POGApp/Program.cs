using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace POGApp
{
    class Program
    {
        private const string IP = "localhost";
        private const int PORT = 9001;

        private static ServiceHost host;

        static void Main(string[] args)
        {
            InitServiceHost();

            Console.ReadLine();
        }

        private static void InitServiceHost()
        {
            Uri tcpAdrs = new Uri("net.tcp://" +
                IP + ":" +
                PORT + "/Host/");

            Uri httpAdrs = new Uri("http://" +
                IP + ":" +
                (PORT + 1).ToString() +
                "/Host/");

            Uri[] baseAdresses = { tcpAdrs, httpAdrs };

            host = new ServiceHost(typeof(POGApp.POGService), baseAdresses);

            NetTcpBinding tcpBinding = new NetTcpBinding(SecurityMode.None, true)
            {
                MaxBufferPoolSize = 67108864,
                MaxBufferSize = 67108864,
                MaxReceivedMessageSize = 67108864,
                TransferMode = TransferMode.Buffered
            };
            tcpBinding.ReaderQuotas.MaxArrayLength = 67108864;
            tcpBinding.ReaderQuotas.MaxBytesPerRead = 67108864;
            tcpBinding.ReaderQuotas.MaxStringContentLength = 67108864;


            tcpBinding.MaxConnections = 100;
            //To maxmize MaxConnections you have 
            //to assign another port for mex endpoint

            //and configure ServiceThrottling as well
            ServiceThrottlingBehavior throttle = host.Description.Behaviors.Find<ServiceThrottlingBehavior>();
            if (throttle == null)
            {
                throttle = new ServiceThrottlingBehavior
                {
                    MaxConcurrentCalls = 100,
                    MaxConcurrentSessions = 100
                };
                host.Description.Behaviors.Add(throttle);
            }


            //Enable reliable session and keep 
            //the connection alive for 20 hours.
            tcpBinding.ReceiveTimeout = new TimeSpan(20, 0, 0);
            tcpBinding.ReliableSession.Enabled = true;
            tcpBinding.ReliableSession.InactivityTimeout = new TimeSpan(20, 0, 10);

            host.AddServiceEndpoint(typeof(POGApp.IPOGService), tcpBinding, "tcp");

            //Define Metadata endPoint, So we can 
            //publish information about the service
            ServiceMetadataBehavior mBehave = new ServiceMetadataBehavior();
            host.Description.Behaviors.Add(mBehave);

            host.AddServiceEndpoint(typeof(IMetadataExchange),
                MetadataExchangeBindings.CreateMexTcpBinding(),
                "net.tcp://" + IP + ":" +
                (PORT - 1).ToString() +
                "/Host/mex");


            try
            {
                host.Open();
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
        }
    }
}
