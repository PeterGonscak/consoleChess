using System;
using System.Linq;
using System.Collections.Generic;

namespace test
{
    static class Program
    {
        static string mainFEN = "RNBQKBNR/PPPPPPPP/8/8/8/8/pppppppp/rnbqkbnr w kqKQ 0 0";          // test psoition 2b3kr/2p1qppp/r1n5/2p5/pb1pB1RP/N4N2/PP1BQPP1/2R3K1 w 0 0
        static Dictionary<char, int> pieceValues = new Dictionary<char, int>(){                    //   constant values
            {'p', 1},
            {'n', 3},
            {'b', 3},
            {'r', 5},
            {'q', 9},
            {'P', -1},
            {'N', -3},
            {'B', -3},
            {'R', -5},
            {'Q', -9},
            {'k', 0},
            {'K', 0},
            {' ', 0}
        };
        static Dictionary<char, int> tileValues = new Dictionary<char, int>(){                   //
            {'a', 0},
            {'b', 1},
            {'c', 2},
            {'d', 3},
            {'e', 4},
            {'f', 5},
            {'g', 6},
            {'h', 7},
            {'1', 56},
            {'2', 48},
            {'3', 40},
            {'4', 32},
            {'5', 24},
            {'6', 16},
            {'7', 8},
            {'8', 0}
        };

        static string[] bw = new string[] { "w", "b" };
        static void Main()
        {

            Console.Write("Enter FEN for game in progress, press ENTER to generate new game.");             //
            string input = Console.ReadLine();                                                             //
            if (input != "")                                                                               //
                if (input.ToCharArray().Count(f => f == ('/')) == 7)                                      //     Input for starting position
                    mainFEN = input;                                                                    //
                else                                                                                   //
                    Console.WriteLine("Wrong Input");                                                 //

            string[] formatFEN = mainFEN.Split(" ");   //
            List<char> board = new List<char>();      //    board generated by FEN
            board = GenerateBoard(formatFEN[0]);     //

            Console.WriteLine("_________________________________________________________\n");    //
            Console.WriteLine("Play by writing move in format: XX(from)XX(to) - ex: b1g6"); //     console graphics
            Console.WriteLine("_________________________________________________________\n");  //
            WriteBoard(board);                                                              //

            while (true)
            {                                        //    main loop for turns
                Console.WriteLine(formatFEN[1] + " to move.");
                while (true)
                {                                                //
                    Console.Write("Enter an valid move: ");                 //     loop for valid move
                    string move = Console.ReadLine();                      //
                    try
                    {                                                                                         //     exception for invalid input
                        if (MakeMove(move, board, char.Parse(formatFEN[1])))
                        {            //     move function  
                            formatFEN[1] = bw[1 - Array.IndexOf(bw, formatFEN[1])];                                       //
                            WriteBoard(board);                          //     update board after move
                            break;
                        }
                    }
                    catch { }
                }
            }
        }

        static bool MakeMove(string s, List<char> board, char onTurn)
        {
            int sPos = TileToNum(s.Substring(0, 2));
            int ePos = TileToNum(s.Substring(2, 2));
            if (((onTurn == 'w' && char.IsLower(board[sPos])) || (onTurn == 'b' && char.IsUpper(board[sPos]))) && IsValid(sPos, ePos, board)){
                board[ePos] = board[sPos];
                board[sPos] = ' ';
                return true;
            }
            return false;
        }

        static bool IsValid(int sPos,int ePos, List<char> board)
        {
            if(board[sPos] == 'p')
                if (((sPos == ePos + 8 || (sPos == ePos +16 && sPos > 47)) && board[ePos] == ' ') || (((sPos == ePos + 7 && (sPos + 1) % 8 != 0) || (sPos == ePos + 9 && sPos % 8 != 0)) && board[ePos] != ' ' &&  char.IsUpper(board[ePos])))
                    return true;
                else 
                    return false;
            else if (board[sPos] == 'P')
                if (((sPos == ePos - 8 || (sPos == ePos - 16 && sPos < 16)) && board[ePos] == ' ') || (((sPos == ePos - 7 && sPos % 8 != 0) || (sPos == ePos - 9 && (sPos+ 1) % 8 != 0)) && board[ePos] != ' ' && char.IsLower(board[ePos])))
                    return true;
                else 
                    return false;
            else
                switch (char.ToLower(board[sPos])){
                    case 'n':
                        break;
                    case 'b':
                        break;
                    case 'r':
                        break;
                    case 'q':
                        break;
                    case 'k':
                        break;
            }
            return true;
        }

        static int TileToNum(string s)
        {
            return tileValues[s[0]] + tileValues[s[1]];
        }

        static List<char> GenerateBoard(string FEN)
        {
            List<char> board = new List<char>();
            foreach (string row in FEN.Split("/"))
                foreach (char c in row)
                    if ("12345678".Contains(c))
                        for (int z = 0; z < int.Parse(c.ToString()); z++)
                            board.Add(' ');
                    else
                        board.Add(c);
            return board;
        }

        static void WriteBoard(List<char> board)
        {
            int i = 0;
            int x = 1;
            Console.WriteLine("    -|-----|-----|-----|-----|-----|-----|-----|-----|");
            foreach (char c in board)
            {
                if (i == 0)
                    Console.Write(9 - x + "  ");
                i++;
                Console.Write("  |  " + c);
                if (i == 8)
                {
                    Console.Write("  |\n    -|-----|-----|-----|-----|-----|-----|-----|-----|\n");
                    i = 0;
                    x++;
                }
            }
            Console.WriteLine("\n        A     B     C     D     E     F     G     H");
            Console.WriteLine("_________________________________________________________");
            Console.WriteLine("Evaluation: " + Eval(board));
            Console.WriteLine("FEN: " + GenerateFEN(board));
        }

        static int Eval(List<char> board)
        {
            int sum = 0;
            foreach (char c in board)
                sum += pieceValues[c];
            return sum;
        }

        static string GenerateFEN(List<char> board)
        {
            string FEN = "";
            int tileCount = 0;
            for (int i = 0; i < board.Count; i++)
            {
                if (board[i] == ' ')
                    tileCount++;
                else if (tileCount == 0)
                    FEN += board[i];
                else
                {
                    FEN += tileCount.ToString() + board[i];
                    tileCount = 0;
                }
                if ((i + 1) % 8 == 0 && (i + 1) != 64)
                {
                    if (tileCount == 0)
                        FEN += "/";
                    else
                    {
                        FEN += tileCount + "/";
                        tileCount = 0;
                    }
                }
            }
            return FEN;
        }
    }
}
