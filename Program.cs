using System;
using System.Linq;

namespace test
{
    static class Program
    {
        static string mainFEN = "RNBQKBNR/PPPPPPPP/8/8/8/8/pppppppp/rnbqkbnr w kqKQ 0 0";
        static Dictionary<char,int> pieceValues = new Dictionary<char,int>() {
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
        static Dictionary<char,int> tileValues = new Dictionary<char,int>() {
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

        static void Main() {
            string[] formatFEN = mainFEN.Split(" ");
            List<char> board = new List<char>();
            board = GenerateBoard(formatFEN[0]);
            Console.WriteLine("Play by writing move in format: XX(from)XX(to) - ex: b1g6\n");
            WriteBoard(board);
            while(true)
            {
                Console.WriteLine(formatFEN[1]+" to move."); 
                while(true)
                {
                    Console.Write("Enter an valid move: ");
                    string move = Console.ReadLine();
                    try{
                        if(MakeMove(move, board, char.Parse(formatFEN[1])))
                            break;
                    }
                    catch{}
                }   
            }
        }
        
        static bool MakeMove(string s, List<char> board, char onTurn) {
            if(s.Substring(0, 2) != s.Substring(2, 2))
                if(onTurn == 'w' && char.IsLower(board[TileToNum(s.Substring(0, 2))]))
                    return true;
                else if (onTurn == 'b' && char.IsUpper(board[TileToNum(s.Substring(0, 2))]))
                    return true;
            return false;
        }

        static int TileToNum(string s) {
            return tileValues[s[0]] + tileValues[s[1]];
        }

        static List<char> GenerateBoard(string FEN) {
            List<char> board = new List<char>();
            foreach( string row in FEN.Split("/"))
                foreach(char c in row)   
                    if("12345678".Contains(c))
                        for(int z = 0; z < int.Parse(c.ToString()); z++)
                            board.Add(' ');
                    else
                        board.Add(c);
            return board;
        }
        
        static void WriteBoard(List<char> board) {
            int i = 0;
            int x = 1;
            Console.WriteLine("    -|-----|-----|-----|-----|-----|-----|-----|-----|");
            foreach(char c in board)
            {
                if(i == 0)
                    Console.Write(9-x+"  ");
                i++;
                Console.Write("  |  " + c);
                if(i==8)
                {
                    Console.Write("  |\n    -|-----|-----|-----|-----|-----|-----|-----|-----|\n");
                    i = 0;
                    x++;
                }        
            }
            Console.WriteLine("\n        A     B     C     D     E     F     G     H");
            Console.WriteLine("_____________________________________");
            Console.WriteLine("Evaluation: "+Eval(board));
        }

        static int Eval(List<char> board) {
            int sum = 0;
            foreach(char c in board)
                sum += pieceValues[c];
            return sum;
        }

        static string GenerateFEN(List<char> board) {
            string FEN = "";
            foreach(char c in board)
            {
                
            }
            return FEN;
        }
    }
}

