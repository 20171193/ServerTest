using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // DSN (Domain Name System)
            // 이름으로 IP 주소를 찾기
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];  // 첫번째로 찾은 주소를 할당
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            // 휴대폰 설정
            Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                // 문지기와 연락시도 (입장 문의)
                socket.Connect(endPoint);

                Console.WriteLine($"Connected To {socket.RemoteEndPoint.ToString()}");

                // 보낸다 (서버와는 반대로 보내기부터 실행)
                byte[] sendBuff = Encoding.UTF8.GetBytes("Hello World!");
                int sendBytes = socket.Send(sendBuff);

                // 받는다
                byte[] recvBuff = new byte[1024];
                int recvBytes = socket.Receive(recvBuff);
                string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                Console.WriteLine($"[From Server] {recvData}");

                // 나간다 (서버에서 쫓아내더라도 추가)
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
