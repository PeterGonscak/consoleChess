using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace consoleChess
{
    public static class DebugTools
    {
        public static void WriteChecks(byte[,] checkBoard)
        {
            for (int x = 0; x < 2; x++)
                for (int i = 0; i < 64; i++)
                {
                    if (i % 8 == 0)
                        Console.WriteLine("");
                    Console.Write(checkBoard[x, i] + "   ");
                }
            
        }
    }
}
