using Server;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class ClientSession : PacketSession
    {
        public int SessionId { get; set; }
        public GameRoom Room { get; set; }  

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"Client OnConnected : {endPoint}");

            Program.Room.Push(() => Program.Room.Enter(this));
        }
        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            SessionManager.Instance.Remove(this);

            if(Room != null)
            {
                GameRoom room = Room;
                room.Push(() => room.Leave(this));
                // Action 참조이므로 단순 null 처리를 하면 안됨.
                Room = null;
            }

            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override void OnSend(int numOfBytes)
        {


            //Console.WriteLine($"Transferred bytes : {numOfBytes}");
        }
    }

}
