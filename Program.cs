using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Traceroute
{
    public class Program
    {
        static void Main(string[] args)
        {
            //string remoteHost = "www.google.com";
            string remoteHost = "www.discovery.com";
            //string remoteHost = "www.yandex.ru";
            Socket host = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
            IPHostEntry ip = Dns.Resolve(remoteHost);
            IPEndPoint ipEndPoint = new IPEndPoint(ip.AddressList[0], 0);
            //IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("202.192.0.2"), 0);
            //IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("210.184.35.3"), 0);
            EndPoint ep = (EndPoint)ipEndPoint;

            host.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 5000);

            int count = 256;
            int complete = 0;
            int badCount = 0;
            int maxTryHop = 10;
            for (byte i = 1; i < count; i++)
            {
                host.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, i);
                ICMP packet = CreateIcmpPacket(i);

                DateTime timestart = DateTime.Now;
                host.SendTo(packet.GetBytes(), packet.messageSize + 4, SocketFlags.None, ipEndPoint);
                try
                {
                    byte[] data = new byte[1024];
                    int recv = host.ReceiveFrom(data, ref ep);
                    TimeSpan timeStop = DateTime.Now - timestart;
                    ICMP response = new ICMP(data, recv);
                    complete++;
                    if (response.type == 11)
                    {
                        Console.WriteLine(i + ": " + ep.ToString() + " " + (timeStop.Milliseconds.ToString()));
                    }

                    if (response.type == 0)
                    {
                        Console.WriteLine(ep.ToString() + " достигнут за " + i + " прыжков, " + (timeStop.Milliseconds.ToString()) + "мс\n");
                        break;
                    }
                    badCount = 0;
                }
                catch (SocketException se)
                {
                    Console.WriteLine(i + ": нет ответа от " + ep + " (" + ipEndPoint + ") - " + Convert.ToString(host.Ttl) + "\n");
                    badCount++;

                    if (badCount == maxTryHop)
                    {
                        Console.WriteLine("Не удалось установить соединение\n");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            Console.WriteLine("ok");
            host.Close();
            //Console.WriteLine($"\nСтатистика Ping для {ip.AddressList[0]}:\n\tПакетов: отправлено: {count}, получено = {complete}, потеряно = {count - complete}\n" +
            //    $"({(count - complete) * 100 / count}% потерь)");
            Console.ReadLine();
        }

        private static ICMP CreateIcmpPacket(byte id)
        {
            byte[] data = new byte[1024];
            ICMP packet = new ICMP();

            packet.id = id;
            packet.type = 0x08;
            packet.code = 0x00;
            packet.checkSum = 0;
            Buffer.BlockCopy(BitConverter.GetBytes(1), 0, packet.message, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(1), 0, packet.message, 2, 2);
            data = Encoding.ASCII.GetBytes($"test packet with id = {id}");
            Buffer.BlockCopy(data, 0, packet.message, 4, data.Length);

            packet.messageSize = data.Length + 4;
            int packetsize = packet.messageSize + 4;

            UInt16 checksum = packet.GetCheckSum();
            packet.checkSum = checksum;
            return packet;
        }
    }
}
