﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
namespace ServerCore
{
    // 재귀적 락 허용 X
    // 스핀락 정책 (5000번 스핀 후 Yield)
    class Lock
    {
        // 비트 플래그 (재귀적 락)
        const int EMPTY_FLAG = 0x00000000;

        const int WRITE_MASK = 0x7FFF0000;
        // 0(mst)111 1111 1111 1111
        // 0000 0000 0000 0000

        const int READ_MASK = 0x0000FFFF;
        // 0000 0000 0000 0000
        // 1111 1111 1111 1111

        const int MAX_SPIN_COUNT = 5000;

        // [Unused(1)] [WriteThreadID(15) [ReadCount(16)]]
        int _flag;

        public void WriteLock()
        {
            // EMPTY_FLAG일 때
            // 즉, 아무도 Write/ReadLock을 획득하지 않을 때, 경합하여 소유권 획득
            int desired = (Thread.CurrentThread.ManagedThreadId << 16) & WRITE_MASK;

            while (true)
            {
                // 5000번 스핀
                for (int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    /// 부적절한 코드 (멑티 스레드 환경에서 원자적 연산을 보장받지 못함)
                    // if(_flag == EMPTY_FLAG)
                    //     _flag = desired;

                    /// 적절한 코드
                    // flag는 획득한 Thread의 Id로 할당됨.
                    if (Interlocked.CompareExchange(ref _flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
                        return;
                }

                Thread.Yield();
            }
        }

        public void WriteUnlock()
        {
            // 빈 플래그로 할당
            Interlocked.Exchange(ref _flag, EMPTY_FLAG);
        }

        public void ReadLock()
        {
            // 아무도 WriteLock을 획득하고 있지 않으면, ReadCount를 1 늘린다.
            while (true)
            {
                for (int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    /// 부적절한 코드 (멑티 스레드 환경에서 원자적 연산을 보장받지 못함)
                    // if ((_flag & WRITE_MASK) == 0)
                    // {
                    //     _flag = _flag + 1;
                    //     return;
                    // }

                    /// 적절한 코드
                    // (내 플래그를 찾을 때까지 경합?)
                    int expected = (_flag & READ_MASK);
                    if (Interlocked.CompareExchange(ref _flag, expected + 1, expected) == expected)
                        return;
             }

                Thread.Yield();
            }
        }
    }
}