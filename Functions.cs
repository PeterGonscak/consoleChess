using System;
using System.Collections.Generic;
using System.Linq;

namespace test
{
    public static class Functions
    {
        public static readonly Dictionary<char, int> pieceValues = new Dictionary<char, int>(){
            {'p', 1}, {'P', -1},
            {'n', 3}, {'N', -3},
            {'b', 3}, {'B', -3},
            {'r', 5}, {'R', -5},
            {'q', 9}, {'Q', -9},
            {'k', 0}, {'K', 0},
            {' ', 0}
        };
        public static readonly Dictionary<char, int> tileValues = new Dictionary<char, int>(){
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
        public static string mainFEN = "RNBQKBNR/PPPPPPPP/8/8/8/8/pppppppp/rnbqkbnr w kqKQ - 0 0";
        public static bool large;
        public static bool IsEnemy(string onTurn, char piece)
        {
            return (onTurn == "b" && char.IsLower(piece))
                || (onTurn == "w" && char.IsUpper(piece));
        }
        public static bool IsFriend(string onTurn, char piece)
        {
            return (onTurn == "w" && char.IsLower(piece))
                || (onTurn == "b" && char.IsUpper(piece));
        }
        public static int Eval(List<char> board)
        {
            int sum = 0;
            foreach (char c in board)
                sum += pieceValues[c];
            return sum;
        }
        public static string GenerateFEN(List<char> board, string[] formatFEN)
        {
            string FEN = "";
            int tileCount = 0;
            for (int i = 0; i < 64; i++)
            {
                if (board[i] == ' ')
                    tileCount++;
                else
                {
                    FEN += (tileCount == 0 ? "" : tileCount.ToString()) + board[i];
                    tileCount = 0;
                }
                if (i % 8 == 7)
                    {
                        FEN += (tileCount == 0 ? "" : tileCount) + (i != 63 ? "/" : "");
                        tileCount = 0;
                    }
            }
            return FEN += " " + formatFEN[1] + " " + formatFEN[2] + " " + formatFEN[3] + " " + formatFEN[4] + " " + formatFEN[5];
        }
        public static int TileToNum(string s)
        {
            return tileValues[s[0]] + tileValues[s[1]];
        }
        public static List<char> GenerateBoard(string FEN)
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

        public static string StartDialog()
        {
            Console.Clear();
            Console.Write("Enter custom valid FEN for game in progress, other input is new game: ");
            string FEN = Console.ReadLine();
            if (FEN.ToCharArray().Count(f => f == ('/')) == 7)
            {
                mainFEN = FEN;
                Console.WriteLine($"{Graphics.line}\nInput Accepted. \n{Graphics.line}");
            }
            else
                Console.WriteLine($"{Graphics.line}\nWrong Input. \n{Graphics.line}");
            while (true)
            {
                Console.Write("Do you want large graphics ? [yes/no]: ");
                string input = Console.ReadLine();
                if (input == "yes" || input == "no")
                {
                    large = input == "yes" ? true : false;
                    break;
                }
            }
            Console.Clear();
            return mainFEN;
        }

        public static char SelectPiece(string onTurn)
        {
            string s = " ";
            while ((s.Length != 1 && !"nbrq".Contains(s)) || s == " ")
            {
                Console.Write("Type to which piece do you want to promote the pawn. [n / b / r / q]: ");
                s = Console.ReadLine();
            }
            return char.Parse(onTurn == "w" ? s : s.ToUpper());
        }
    }
}
