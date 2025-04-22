using ServerCore;
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
        public static GameRoom Room = new GameRoom();

        static void Main(string[] args)
        {
            // DSN (Domain Name System)
            // 이름으로 IP 주소를 찾기
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];  // 첫번째로 찾은 주소를 할당
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            // 콜백 방식 사용
            _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
            Console.WriteLine("Listening....");

            int roomTick = 0;
            while (true)
            {
                // 기존방식 
                // 1. Thread.Sleep(250);
                // * 각 상황마다 시간은 다를 수 있음

                // 2. tick을 사용해서 관리
                // int now = System.Environment.TickCount;
                // if(roomTick < now)
                // {
                //     Room.Push(() => Room.Flush());
                //     roomTick = now + 250;
                // }
                // * 모든 이벤트를 tick으로 관리할 경우
                //   매 프레임마다 불필요한 연산이 생김.

                // 개선방식
                // Priority Queue

            }
        }
    }
}
