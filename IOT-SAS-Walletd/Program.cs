using System;
using System.Threading;
using FactomSharp.Factomd;
using Nancy.Bootstrapper;
using Nancy.Hosting.Self;

namespace IOTSASWalletd
{
    public class MainClass
    {
        static AutoResetEvent KeepRunning = new AutoResetEvent(false);
        
        
        public static void Main(string[] args)
        {
    
            using (IOT_SAS iotsas = new IOT_SAS("/dev/ttyUSB0"))
            {
            
                //var ecAddress = iotsas.GetECAddress(null);
                //Console.WriteLine($"Public ECAddress: {ecAddress.Public}");
            
                HostConfiguration hostConf = new HostConfiguration();
                hostConf.RewriteLocalhost = true;
                
            
                var uri = new Uri("http://localhost:8089");
                using (var host = new NancyHost(uri))
                {
                    host.Start();
                    Console.WriteLine($"Starting faux walletd on {uri}");

   
                    var rest = new Rest();


                    KeepRunning.WaitOne();

                }
            }
        }
        
    }
}
