using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using ServerCore;
using System.Runtime.InteropServices;

namespace DummyClient
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

            Connector connector = new Connector();

            connector.Connect(endPoint, () => { return SessionManager.Instance.Generate(); }
            , 10);

            while (true)
            {
                try
                {
                    /************************** 수정 이전 ***************************
                    /* 문지기와 연락시도 (입장 문의)
                    /** 블로킹 방식으로 수정이 필요
                    socket.Connect(endPoint);
                    *****************************************************************/

                    SessionManager.Instance.SendForEach();
                
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                // 보통 1초에 4번정도 패킷을 보냄
                Thread.Sleep(250);
            }
        }
    }
}
