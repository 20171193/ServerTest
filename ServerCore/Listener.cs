﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class Listener
    {
        private Socket _listenSocket;

        private Action<Socket> _onAcceptHandler;

        public void Init(IPEndPoint endPoint, Action<Socket> onAcceptHandler)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _onAcceptHandler = onAcceptHandler; 
            // 문지기 교육 : 바인딩
            _listenSocket.Bind(endPoint);

            // 영업 시작
            // backlog : 최대 대기수
            _listenSocket.Listen(10);

            // 최초 생성 후 재사용
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            // 최초 한번 등록실행
            RegisterAccept(args);
        }

        private void RegisterAccept(SocketAsyncEventArgs args)
        {
            // 기존의 할당된 소켓을 초기화해야함. (재등록 시 에러 방지)
            args.AcceptSocket = null;

            // 논블로킹 함수로 전환 (비동기 작업으로 전환)
            //  but 코드의 흐름이 복잡해 짐.
            bool pending = _listenSocket.AcceptAsync(args);
            // 펜딩없이 비동기 작업이 바로 완료된 경우
            if (pending == false)
                OnAcceptCompleted(null, args);
        }
        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            if(args.SocketError == SocketError.Success)
            {
                _onAcceptHandler.Invoke(args.AcceptSocket);
            }
            else
                Console.WriteLine(args.SocketError.ToString());

            // 완료 후 다음을 위해 재등록
            RegisterAccept(args);
        }

        public Socket Accept()
        {
            // **이는 블로킹 계열의 함수로 스레드가 차단되게 됨.
            //return _listenSocket.Accept();

            return _listenSocket.Accept();
        }
    }
}