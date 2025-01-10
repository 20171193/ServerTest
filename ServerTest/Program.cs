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
    class Test
    {
        public int hp = 0;
        public int atk = 0;
    }

    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected : {endPoint}");

            Test test = new Test() { hp = 100, atk = 10 };

            ArraySegment<byte>? openSegment = SendBufferHelper.Open(4096);
            byte[] buffer = BitConverter.GetBytes(test.hp);
            byte[] buffer2 = BitConverter.GetBytes(test.atk);
            Array.Copy(buffer, 0, openSegment.Value.Array, openSegment.Value.Offset, buffer.Length);
            Array.Copy(buffer2, 0, openSegment.Value.Array, openSegment.Value.Offset + buffer.Length, buffer2.Length);
            ArraySegment<byte> sendBuff = SendBufferHelper.Close(buffer.Length + buffer2.Length);

            Send(sendBuff);
            Thread.Sleep(1000);
            Disconnect();
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Client] : {recvData}");
            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes : {numOfBytes}");
        }
    }

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
            _listener.Init(endPoint, () => { return new GameSession(); });
            Console.WriteLine("Listening....");

            // Danger Zone (공유 자원을 다룰 경우 동기화 문제가 발생할 수 있는 구역)
            while (true)
            {

            }
        }
    }
}
