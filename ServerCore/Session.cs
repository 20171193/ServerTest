using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    public class Session
    {
        private Socket _socket;
        private int _disconnected = 0;

        private Queue<byte[]> _sendQueue = new Queue<byte[]>();
        private bool _pending = false;
        private object _lock = new object();
        private SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();

        public void Start(Socket socket)
        {
            _socket = socket;

            SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);

            // SetBuffer Params
            //  : 버퍼 인덱스가 꼭 0으로 시작하지 않을 수 있음.
            //  : 큰 버퍼에서 일정 영역만 사용하는 경우도 존재
            recvArgs.SetBuffer(new byte[1024], 0, 1024);

            RegisterRecv(recvArgs);
        }

        public void Send(byte[] sendBuff)
        {
            /************************** 수정 이전 *****************************************
             * Send시 마다 계속해서 생산? (재사용이 불가하므로 성능상의 문제가 발생)
            SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();
            sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
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
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuff);
                if (_pending == false)
                    RegisterSend();
            }
        }

        public void Disconnect()
        {
            // 연속 Disconnect 방지
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
                return;

            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        #region 네트워크 통신

        private void RegisterSend()
        {
            // 펜딩여부를 통해 예약상태 확인
            _pending = true;

            byte[] buff = _sendQueue.Dequeue();
            _sendArgs.SetBuffer(buff, 0, buff.Length);

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

                        if(_sendQueue.Count > 0)
                            // 락을 통한 예약대기 상태의 버퍼 처리
                            RegisterSend();
                        else
                            _pending = false;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"OnSendCompleted Failed {ex.ToString()}");
                    }
                }
            }
        }
        private void RegisterRecv(SocketAsyncEventArgs args)
        {
            bool pending = _socket.ReceiveAsync(args);
            if (pending == false)
                OnRecvCompleted(null, args);
        }

        private void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    string recvData = Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);
                    Console.WriteLine($"[From Client] : {recvData}");
                    RegisterRecv(args);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"OnRecvCompleted Failed : {ex.ToString()}");
                }
            }
            // 해제해야 함.
            else
            {

            }
        }
        #endregion
    }
}
