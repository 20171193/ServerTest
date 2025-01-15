using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    /************************ Send Buffer vs Recv Buffer *************************
     * 클라이언트의 요청사항은 각기 다를 것.
     ** 따라서 Session은 고유의 Recv Buffer를 내부적으로 가짐. (1:1 관계)
     
     * Send Buffer는 모든 이용자가 패킷을 보내기 때문에 작업이 빈번함.
     ** 내부적으로 관리한다면 복사가 매우 많이 일어남.
     ** 외부에서 만들어 바로 보내는 방식으로 구성하는 것이 효율적임.
     *** 그렇다면 각기 다른 패킷에 대한 Send Buffer의 크기는? 
     *****************************************************************************/

    public class SendBufferHelper
    {
        // ThreadLocal : 컨텐츠 간의 경합을 방지 하기위함.
        public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>(() => { return null; });

        public static int ChunkSize { get; set; } = 4096 * 100;

        public static ArraySegment<byte> Open(int reserveSize)
        {
            if (CurrentBuffer.Value == null || CurrentBuffer.Value.FreeSize < reserveSize)
                CurrentBuffer.Value = new SendBuffer(ChunkSize);

            return CurrentBuffer.Value.Open(reserveSize);
        }
        public static ArraySegment<byte> Close(int usedSize)
        {
            return CurrentBuffer.Value.Close(usedSize);
        }
    }

    public class SendBuffer
    {
        // [] [] [] [] [] [] [] [] [] [] 
        private byte[] _buffer;
        // SendBuffer의 커서는 한 개
        private int _usedSize = 0;
        // Pending Queue에 버퍼가 담겨있을 수 있기 때문에
        // 커서를 이동하는 것은 위험함.

        public SendBuffer(int chunkSize)
        {
            _buffer = new byte[chunkSize];
        }

        public int FreeSize { get { return _buffer.Length - _usedSize; } }

        public ArraySegment<byte> Open(int reserveSize)
        {
            if (reserveSize > FreeSize)
                return null;

            return new ArraySegment<byte>(_buffer, _usedSize, reserveSize);
        }
        public ArraySegment<byte> Close(int usedSize)
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(_buffer, _usedSize, usedSize);
            _usedSize += usedSize;
            return segment;
        }
    }
}
