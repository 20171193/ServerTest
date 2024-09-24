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
        static void Main(string[] args)
        {
            int[,] arr = new int[10000, 10000];

            // x부터 탐색 : 주변 메모리 주소를 캐싱
            // [1] [next:캐싱] [] []
            // [] [] [] []
            long now = DateTime.Now.Ticks;
            for (int y = 0; y < 10000; y++)
                for(int x = 0; x<10000; x++)
                    arr[y, x] = 1;
            long end = DateTime.Now.Ticks;
            Console.WriteLine(end - now);

            // y부터 탐색 : 상대적으로 공간적 접근성이 더 낮음.
            // [1] [캐싱] [] []
            // [next] [] [] []
            now = DateTime.Now.Ticks;
            for (int y = 0; y < 10000; y++)
                for (int x = 0; x < 10000; x++)
                    arr[x, y] = 1;
            end = DateTime.Now.Ticks;
            Console.WriteLine(end - now);
        }
    }
}
