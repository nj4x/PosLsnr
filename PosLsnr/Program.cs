using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using nj4x;
using nj4x.Metatrader;
using NLog;
using NLog.Config;

namespace PosLsnr
{
    public class Program
    {
        static Program()
        {
            LogManager.Configuration = new XmlLoggingConfiguration(".\\NLog.config");
        }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            var terminalServerHost = "127.0.0.1";
            var terminalServerPort = 7788;
            //
            string brokerName = "5*91.109.206.235:443"; //Christian's acct = 5*45.76.131.24,50040583,Cajt4j2z
            string account = "9007162";
            string password = "ibiq4mnf";
            //
            if (args.Length > 0)
            {
                // ts:port broker,acct,pwd [nj4x_server_port]
                var tsInfo = args[0].Split(':');
                terminalServerHost = tsInfo[0];
                terminalServerPort = int.Parse(tsInfo[1]);
                //
                var acctInfo = args[1].Split(',');
                brokerName = acctInfo[0];
                account= acctInfo[1];
                password= acctInfo[2];
                //
                if(args.Length == 3)
                    ConfigurationManager.AppSettings.Set("nj4x_server_port", args[2]);
            }
            //
            var mt4 = new Strategy();
            //
            var plCallNo = 0;
            mt4.SetPositionListener(
                delegate (IPositionInfo info)
                {
                    Logger.Info(account+"> ----------------------------------------------------------------------");
                    Logger.Info(account+"> initialPositionInfo=" + info);
                    Logger.Info(account+"> Historical Orders: ");
                    foreach (IOrderInfo o in info.HistoricalOrders.Values)
                    {
                        Logger.Info(account+"> HO: " + o);
                    }
                    Logger.Info(account+"> Live working Orders");
                    foreach (IOrderInfo o in info.LiveOrders.Values)
                    {
                        Logger.Info(account+"> LO: " + o + " CloseTime=" + o.GetCloseTime() + " ClosePrice=" + o.GetClosePrice());
                    }
                    Logger.Info(account+"> ----------------------------------------------------------------------");
                    Thread.Sleep(1000);
                },
                delegate (IPositionInfo info, IPositionChangeInfo changes)
                {
                    Logger.Info(account+"> ----------------------------------------------------------------------");
                    Logger.Info($"get Nj4x OrderUpdate #{++plCallNo} || Closed: " + changes.GetClosedOrders().Count + " Modified: " + changes.GetModifiedOrders().Count + " New: " + changes.GetNewOrders().Count );
                    Logger.Info(account+"> Historical Orders: ");
                    foreach (IOrderInfo o in info.HistoricalOrders.Values)
                    {
                        Logger.Info(account+"> HO: " + o);
                    }
                    Logger.Info(account+"> Live working Orders");
                    foreach (IOrderInfo o in info.LiveOrders.Values)
                    {
                        Logger.Info(account+"> LO: " + o + " CloseTime=" + o.GetCloseTime() + " ClosePrice=" + o.GetClosePrice());
                    }
                    Logger.Info(account+"> ---------- changes:");
                    foreach (IOrderInfo o in changes.GetNewOrders())
                    {
                        Logger.Info(account+"> PosInfo NEW: " + o);
                    }
                    foreach (IOrderInfo o in changes.GetModifiedOrders())
                    {
                        Logger.Info(account+"> PosInfo MODIFIED: " + o);
                    }
                    foreach (IOrderInfo o in changes.GetClosedOrders())
                    {
                        Logger.Info(account+"> PosInfo CLOSED: " + o);
                    }
                    foreach (IOrderInfo o in changes.GetDeletedOrders())
                    {
                        Logger.Info(account+"> PosInfo DELETED: " + o);
                    }
                    Logger.Info(account+"> ----------------------------------------------------------------------");
                    return Task.FromResult(0);
                }
            );
            //
            mt4.Connect(terminalServerHost, terminalServerPort, new Broker(brokerName), account, password);
            Console.WriteLine("Connected. Check log files...\nTo terminate - press Enter...");
            Console.ReadLine();
        }
    }
}