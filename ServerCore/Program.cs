using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;

namespace ServerCore
{
    class Program
    {
        static void Main(string[] args)
        {
            // DSN (Domain Name System)
            // 이름으로 IP 주소를 찾기
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];  // 첫번째로 찾은 주소를 할당
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            // 문지기 생성 => 리슨 소켓 생성 (TCP 통신)
            Socket listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);


            try
            {
                // 문지기 교육 : 바인딩
                listenSocket.Bind(endPoint);

                // 영업 시작
                // backlog : 최대 대기수
                listenSocket.Listen(10);

                while (true)
                {
                    Console.WriteLine("Listening....");

                    // 손님 입장시키기
                    Socket clientSocket = listenSocket.Accept();

                    // 받기
                    byte[] recvBuff = new byte[1024];
                    int recvBytes = clientSocket.Receive(recvBuff);
                    // 리시브버퍼, 시작 인덱스, 길이
                    string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                    Console.WriteLine($"[From Client] : {recvData}");

                    // 보내기
                    byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to MMORPG Server !");
                    clientSocket.Send(sendBuff);
                    // 쫓아내기
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
