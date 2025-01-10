using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    public abstract class Session
    {
        private Socket _socket;
        private int _disconnected = 0;

        RecvBuffer _recvBuffer = new RecvBuffer(1024);

        private object _lock = new object();

        // Send Arguments
        private Queue<byte[]> _sendQueue = new Queue<byte[]>();
        private List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        private SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        
        // Receive Arguments
        private SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract int OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);

        public void Start(Socket socket)
        {
            _socket = socket;

            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);

            // SetBuffer Params
            //  : 버퍼 인덱스가 꼭 0으로 시작하지 않을 수 있음.
            //  : 큰 버퍼에서 일정 영역만 사용하는 경우도 존재
            // _recvArgs.SetBuffer(new byte[1024], 0, 1024);

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();
        }

        public void Send(byte[] sendBuff)
        {
            /************************** 수정 이전 *****************************************
             * Send시 마다 계속해서 생산? (재사용이 불가하므로 성능상의 문제가 발생)
            SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();
            sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);
            sendArgs.SetBuffer(sendBuff, 0, sendBuff.Length);

             * Send 등록
             ** 이도 마찬가지로 Send 소켓을 계속해서 등록하면 문제가 발생.
             ** Send의 호출 스택이 증가
            RegisterSend(sendArgs);
            ******************************************************************************/

            /************************** 수정 이후 *****************************************
            _sendArgs.SetBuffer(sendBuff, 0, sendBuff.Length);
            RegisterSend(); 
             * sendArgs를 재사용하기위해 클래스 지역변수로 할당
             ** 하지만, Send가 완료되지 않았는데 버퍼를 바꾸는 경우가 발생할 수 있음.
             *** 최종적으로 Send 버퍼를 락과 큐를 활용하여 관리
             *****************************************************************************/

            /*********************************** 개선 ************************************
             * 각각의 Send Buffer를 보내는 것이 아닌, 하나의 큰 Send Buffer를 생성하고,
             * 해당 Buffer를 쪼개서 보낸다면 더 개선이 될 수 있음.
             **********************************************************/

            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuff);
                if (_pendingList.Count == 0)
                    RegisterSend();
            }
        }

        public void Disconnect()
        {
            // 연속 Disconnect 방지
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
                return;

            OnDisconnected(_socket.RemoteEndPoint);
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        #region 네트워크 통신

        private void RegisterSend()
        {
            // _sendArgs.BufferList : 버퍼를 리스트로 만들어 한 번에 전송이 가능함.
            while(_sendQueue.Count > 0)
            {
                byte[] buff = _sendQueue.Dequeue();
                // ArraySegment : 배열의 일부를 나타내는 ***구조체
                // _sendArgs.BufferList.Add(new ArraySegment<byte>(buff, 0, buff.Length));
                // Error : 버퍼리스트를 먼저 만들고 넣어야함.
                _pendingList.Add(new ArraySegment<byte>(buff, 0, buff.Length));
            }
            _sendArgs.BufferList = _pendingList;

            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false)
                OnSendCompleted(null, _sendArgs);
        }
        private void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            // 이벤트 방식으로 호출되는 Completed 메서드도 락으로 관리
            lock (_lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        // Receive의 경우는 Start에서 예약이 되어있음.
                        // **Send는 그 시점이 명확하지 않음
                        // **Receive 버퍼는 사용완료 후 재사용이 가능하지만, Send 버퍼는 재사용이 불가
                        _sendArgs.BufferList = null;
                        _pendingList.Clear();

                        OnSend(_sendArgs.BytesTransferred);

                        if (_sendQueue.Count > 0)
                            // 락을 통한 예약대기 상태의 버퍼 처리
                            RegisterSend();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"OnSendCompleted Failed {ex.ToString()}");
                    }
                }
            }
        }
        private void RegisterRecv()
        {
            // 유효 범위를 확인해야함.
            _recvBuffer.Clean();
            ArraySegment<byte> segment = _recvBuffer.WriteSegment;
            _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            bool pending = _socket.ReceiveAsync(_recvArgs);
            if (pending == false)
                OnRecvCompleted(null, _recvArgs);
        }

        private void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    // Write 커서 이동
                    if(_recvBuffer.OnWrite(args.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }

                    // 컨텐츠 쪽으로 데이터 전송 후 처리상태를 받아옴
                    int processLen = OnRecv(_recvBuffer.ReadSegment);
                    if (processLen < 0 || _recvBuffer.DataSize < processLen)
                    {
                        Disconnect();
                        return;
                    }

                    // Read 커서 이동
                    if(_recvBuffer.OnRead(args.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }

                    RegisterRecv();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"OnRecvCompleted Failed : {ex.ToString()}");
                }
            }
            // 해제해야 함.
            else
            {
                Disconnect();
            }
        }
        #endregion
    }
}
