using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.ComponentModel;

namespace EasyValueMonitor
{

    public class Client
    {
        public ReadMessageBox ReadMessageBox { get; set; }

        public delegate void ResponseReceived(MessageBox mbox);

        public ResponseReceived OnResponseReceived { get; set; }

        public Client()
        {
        }

        public async void Start(string host, int port)
        {
            int wait_time = 0;
            while (true)
            {
                try
                {
                    using (tcpClient = new TcpClient())
                    {
                        await Task.Delay(wait_time);
                        await tcpClient.ConnectAsync(host, port);

                        tcpClient.Client.LingerState = new LingerOption(true, 5);

                        using (var ns = tcpClient.GetStream())
                        {
                            await ReadValue(ns);
                        }
                    }
                }
                catch (Exception)
                {
                    wait_time = 3000;
                }
                finally
                {
                    this.Close();
                }
            }
        }

        private async Task ReadValue(NetworkStream ns)
        {
            while (true)
            {
                await Task.Delay(10);

                MessageBox msgBox;

                if(message_que.Count > 0)
                {
                    msgBox = message_que.Dequeue();
                }
                else
                {
                    msgBox = ReadMessageBox;
                }

                await ns.WriteAsync(msgBox.SendBytes, 0, msgBox.SendBytes.Length);

                do
                {
                    int result_size = await ns.ReadAsync(msgBox.ReplyBytes, 0, msgBox.ReplyBytes.Length);

                    if (result_size == 0)
                    {
                        throw new Exception("Received data is 0");
                    }

                    OnResponseReceived(msgBox);

                } while (ns.DataAvailable);
            }
        }

        public void SetWriteMessageBox(WriteMessageBox mbox)
        {
            message_que.Enqueue(mbox);
        }

        public void Close()
        {
            tcpClient.Close();
        }

        public void Closed(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private TcpClient tcpClient;

        private Queue<MessageBox> message_que = new Queue<MessageBox>();

        ~Client()
        {
            this.Close();
        }
    }



    public class PacketInfo
    {
    }

    class ReadPacketInfo : PacketInfo
    {
        public int Address { get; private set; }
        public int Number { get; private set; }

        public ReadPacketInfo(int address, int number)
        {
            Address = address;
            Number = number;
        }
    }

    class WritePacketInfo : PacketInfo
    {
        public int Address { get; private set; }
        public int Number { get; private set; }
        public int Value { get; private set; }

        public WritePacketInfo(int address, int number, int value)
        {
            Address = address;
            Number = number;
            Value = value;
        }
    }



    public enum MessageType
    {
        Read = 0,
        Write
    };

    public abstract class MessageBox
    {
        public byte[] SendBytes { get; private set; }
        public byte[] ReplyBytes { get; set; }
        public PacketInfo packetInfo { get; private set; }
        public MessageType Type { get; private set; }

        public MessageBox(PacketInfo p)
        {
            packetInfo = p;
            SendBytes = BuildPacket();
            ReplyBytes = new byte[256];
            Type = SetType();
        }

        public abstract int Parse();

        protected abstract byte[] BuildPacket();

        protected abstract MessageType SetType();
    }

    public class ReadMessageBox : MessageBox
    {
        public ReadMessageBox(PacketInfo p) : base(p)
        {
        }

        public override int Parse()
        {
            var tmp = BitConverter.ToInt16(ReplyBytes.Skip(9).Take(2).Reverse().ToArray(), 0);
            return tmp;
        }

        protected override byte[] BuildPacket()
        {
            var readInfo = (ReadPacketInfo)packetInfo;

            var tid = new byte[] { 0x00, 0x00 };
            var pid = new byte[] { 0x00, 0x00 };
            var uid = new byte[] { 0xFF };
            var fc = new byte[] { 0x03 };
            var adr = BitConverter.GetBytes((short)readInfo.Address).Reverse();
            var num = BitConverter.GetBytes((short)readInfo.Number).Reverse();

            var packet_len = uid.Count();
            packet_len += fc.Count();
            packet_len += adr.Count();
            packet_len += num.Count();

            var len = BitConverter.GetBytes((short)packet_len).Reverse();

            return tid.Concat(pid).Concat(len).Concat(uid).Concat(fc).Concat(adr).Concat(num).ToArray();
        }

        protected override MessageType SetType()
        {
            return MessageType.Read;
        }
    }

    public class WriteMessageBox : MessageBox
    {
        public WriteMessageBox(PacketInfo p) : base(p)
        {
        }

        public override int Parse()
        {
            return 0;
        }

        protected override byte[] BuildPacket()
        {
            var writeInfo = (WritePacketInfo)packetInfo;

            var tid = new byte[] { 0x00, 0x00 };
            var pid = new byte[] { 0x00, 0x00 };
            var uid = new byte[] { 0xFF };
            var fc = new byte[] { 0x10 };
            var adr = BitConverter.GetBytes((short)writeInfo.Address).Reverse();
            var num = BitConverter.GetBytes((short)writeInfo.Number).Reverse();
            var byte_count = new byte[] { BitConverter.GetBytes((short)(writeInfo.Number * 2))[1] };
            var value = BitConverter.GetBytes((short)writeInfo.Value).Reverse();

            var packet_len = uid.Count();
            packet_len += fc.Count();
            packet_len += adr.Count();
            packet_len += num.Count();
            packet_len += 1;
            packet_len += value.Count();

            var len = BitConverter.GetBytes((short)packet_len).Reverse();

            return tid.Concat(pid).Concat(len).Concat(uid).Concat(fc).Concat(adr).Concat(num).Concat(byte_count).Concat(value).ToArray();

        }

        protected override MessageType SetType()
        {
            return MessageType.Write;
        }
    }
}
