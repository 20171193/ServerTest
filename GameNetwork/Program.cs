﻿using ServerCore;
using ServerTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Server
{
    class Program
    {
        static Listener _listener = new Listener();

        static void Main(string[] args)
        {
            // DSN (Domain Name System)
            // 이름으로 IP 주소를 찾기
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];  // 첫번째로 찾은 주소를 할당
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);


            // 콜백 방식 사용
            _listener.Init(endPoint, () => { return new ClientSession(); });
            Console.WriteLine("Listening....");

            // Danger Zone (공유 자원을 다룰 경우 동기화 문제가 발생할 수 있는 구역)
            while (true)
            {

            }
        }
    }
}