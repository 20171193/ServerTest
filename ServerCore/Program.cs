using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class Program
    {
        int _answer;
        bool _complete;

        void A()
        {
            _answer = 123;      // Store
            // Store 후 베리어
            Thread.MemoryBarrier(); // B 1
            _complete = true;   // Store
            // 가시성 보장
            // Store 후 베리어
            Thread.MemoryBarrier(); // B 2
        }
        void B()
        {
            // Load 전 베리어
            Thread.MemoryBarrier(); // B 3
            if (_complete)  // Load
            {
                Thread.MemoryBarrier(); // B 4
                Console.WriteLine(_answer);
            }
        }


        static void Main(string[] args)
        {
        }
    }
}
