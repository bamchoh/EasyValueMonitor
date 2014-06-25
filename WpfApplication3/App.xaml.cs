using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace EasyValueMonitor
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        public App()
            : base()
        {
            this.Startup += new StartupEventHandler(Application_Startup);
        }

        [STAThread]
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var window = new MainWindow();

            // window.Closed += new EventHandler(driverManager.Closed);

            window.DataContext = new MainWindowViewModel();

            this.MainWindow.Show();
        }

    }


    public interface IPacketBuilder
    {
        void Init();

        byte[] Read(PacketInfo packetInfo);

        byte[] Write(PacketInfo packetInfo);
    }



    public interface IPacketParser
    {
        byte[] Parse(PacketInfo packetInfo);
    }



    public abstract class CommManager
    {
        public abstract void Closed(object sender, System.EventArgs e);
    }



    public class ModbusTCPMasterPacketBuilder : IPacketBuilder
    {
        void IPacketBuilder.Init()
        {
            throw new NotImplementedException();
        }

        byte[] IPacketBuilder.Read(PacketInfo packetInfo)
        {
            throw new NotImplementedException();
        }

        byte[] IPacketBuilder.Write(PacketInfo packetInfo)
        {
            throw new NotImplementedException();
        }
    }



    public class ModbusTCPMasterPacketParser : IPacketParser
    {
        byte[] IPacketParser.Parse(PacketInfo packetInfo)
        {
            throw new NotImplementedException();
        }
    }



    public class ModbusTCPMasterCommManager : CommManager
    {
        public Client Client { get; private set; }

        public ModbusTCPMasterCommManager(DriverConfiguration driverConfig) : base()
        {
            // Client = new Client(driverConfig);
        }

        public override void Closed(object sender, System.EventArgs e)
        {
            Client.Closed(sender, e);
        }
    }



    public abstract class DriverManager
    {
        public string Name = null;

        public virtual void Init(DriverConfiguration driverConfig)
        {
            packetBuilder = GetPacketBuilder();
            packetParser = GetPacketParser();
            CommManager = GetCommManager(driverConfig);
        }

        public virtual void Closed(object sender, System.EventArgs e)
        {
            CommManager.Closed(sender, e);
        }

        protected IPacketBuilder packetBuilder;
        protected IPacketParser packetParser;
        public CommManager CommManager { get; private set; }

        protected abstract IPacketBuilder GetPacketBuilder();
        protected abstract IPacketParser GetPacketParser();
        protected abstract CommManager GetCommManager(DriverConfiguration driverConfig);
    }



    public class ModbusTCPMasterDriverManager : DriverManager
    {
        public ModbusTCPMasterDriverManager()
        {
            Name = "ModbusTCPMaster";
        }

        protected override IPacketBuilder GetPacketBuilder()
        {
            return new ModbusTCPMasterPacketBuilder();
        }

        protected override IPacketParser GetPacketParser()
        {
            return new ModbusTCPMasterPacketParser();
        }

        protected override CommManager GetCommManager(DriverConfiguration driverConfig)
        {
            return new ModbusTCPMasterCommManager(driverConfig);
        }
    }



    public class DriverConfiguration
    {
        public string Name { get; set; }
        public string PortType { get; set; }
        public string IpAddress { get; set; }
        public int PortNo { get; set; }

        public DriverConfiguration()
        {
            Name = "ModbusTCPMaster";
            PortType = "TCP";
            IpAddress = "127.0.0.1";
            PortNo = 502;
        }
    }



    public static class SystemGenerator
    {
        public static DriverManager GetDriverManager(DriverConfiguration driverConfig)
        {
            var driverManagerName = "WpfApplication3." + driverConfig.Name + "DriverManager";
            Type driverType = Type.GetType(driverManagerName);
            return (DriverManager)Activator.CreateInstance(driverType);
        }
    }
}
