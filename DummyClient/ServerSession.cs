using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient
{

    class Packet
    {
        public ushort size;
        public ushort packetId;
    }
    class PlayerInfoReq : Packet
    {
        public long playerId;
    }
    class PlayerInfoOk : Packet
    {
        public int hp;
        public int atk;
    }
    public enum PacketID
    {
        PlayerInfoReq = 1,
        PlayerInfoOk = 2,
    }

    class ServerSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected : {endPoint}");

            PlayerInfoReq packet = new PlayerInfoReq() { size = 4, packetId = (ushort)PacketID.PlayerInfoReq };

            //for (int i = 0; i < 5; i++)
            {
                ArraySegment<byte> s = SendBufferHelper.Open(4096);

                ushort count = 0;
                bool success = true;
                
                success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset))

                byte[] size = BitConverter.GetBytes(packet.size);
                byte[] packetId = BitConverter.GetBytes(packet.packetId);
                byte[] playerId = BitConverter.GetBytes(packet.playerId);


                Array.Copy(size, 0, s.Value.Array, s.Value.Offset + count, 2);
                count += 2;
                Array.Copy(packetId, 0, s.Value.Array, s.Value.Offset + count, 2);
                count += 2;
                Array.Copy(playerId, 0, s.Value.Array, s.Value.Offset + count, 2);
                count += 8;
                ArraySegment<byte> sendBuff = SendBufferHelper.Close(packet.size);

                Send(sendBuff);
                }
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Server] : {recvData}");
            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes : {numOfBytes}");
        }
    }
}
