using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace EasyValueMonitor
{
    public class MainWindowModel
    {
        public DataList DataList { get; private set; }

        public int Number
        {
            get { return 1; }
        }

        public MainWindowModel()
        {
            var driverConfig = new DriverConfiguration();

            var systemGen = SystemGenerator.GetDriverManager(driverConfig);

            DataList = new DataList { Address = 0, Value = 0 };

            this.DataList.PropertyChanged += DataListPropertyChanged;

            client = new Client();

            client.OnResponseReceived = ResponseReceived;

            ReadMessageBoxSet();

            client.Start(driverConfig.IpAddress, driverConfig.PortNo);
        }

        private Client client;

        private void DataListPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Address":
                    ReadMessageBoxSet();
                    break;
                case "WriteValue":
                    var p = new WritePacketInfo(this.DataList.Address, Number, this.DataList.WriteValue);
                    var mbox = new WriteMessageBox(p);
                    client.SetWriteMessageBox(mbox);
                    break;
                default:
                    break;
            }

        }

        private void ReadMessageBoxSet()
        {
            var p = new ReadPacketInfo(DataList.Address, Number);
            client.ReadMessageBox = new ReadMessageBox(p);
        }

        private void ResponseReceived(MessageBox mbox)
        {
            switch (mbox.Type)
            {
                case MessageType.Read:
                    this.DataList.Value = mbox.Parse();
                    break;
                case MessageType.Write:
                    break;
                default:
                    break;
            }
        }

    }

}
