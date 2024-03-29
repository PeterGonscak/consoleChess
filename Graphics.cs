using System;
using System.Collections.Generic;
using System.Linq;

namespace consoleChess
{
    public static class Graphics
    {
        public static int i = 0;
        public static int x = 1;
        public static string rowSplit = "      -|-----+-----+----+-----+-----+-----+-----+-----+-";
        public static string letterCords = "          A     B     C     D     E     F     G     H";
        public static string midTileEdge = "  |  ";
        public static string tileEdge = "";
        public static string rowSplitLarge = "        -+---------+---------+---------+---------+---------+---------+---------+---------+-";
        public static string letterCordsLarge = "              A         B         C         D         E         F         G         H";
        public static string midTileEdgeLarge = "    |    ";
        public static string tileEdgeLarge = "         |         |         |         |         |         |         |         |         | ";
        public static string line = "________________________________________________________________________________________________\n";
        public static string mainFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 0"; //"1N1KQ1R1/PPPP2PP/5P2/5p2/7k/1BB5/pppp1NRp/rnbq1bnr b kqKQ - 0 0";
        public static bool large;

        
        public static void WriteHint()
        {
            Console.WriteLine(line);
            Console.WriteLine("Play by writing move in format: XX(from)XX(to) - ex: d4e5");
            Console.WriteLine("For resignation write 'resign'");
            Console.WriteLine(line);  
        }
        public static void WriteBoard(List<char> board, string[] formatFEN)
        {
            int i = 0;
            int x = 1;
            string rowSplit = "      -+-----+-----+-----+-----+-----+-----+-----+-----+-";
            string letterCords = "          A     B     C     D     E     F     G     H";
            string midTileEdge = "  |  ";
            string tileEdge = "";
            if (large)
            {
                rowSplit = "        -+---------+---------+---------+---------+---------+---------+---------+---------+-";
                letterCords = "              A         B         C         D         E         F         G         H";
                midTileEdge = "    |    ";
                tileEdge = "         |         |         |         |         |         |         |         |         | ";
            }
            Console.WriteLine($"\n {letterCords} {(large ? "\n" : "" )}");
            Console.WriteLine(rowSplit);
            Console.Write(tileEdge + (large ? "\n" : ""));
            foreach (char c in board)
            {
                if (i == 0)
                    Console.Write($"    {9 - x}");
                i++;
                Console.Write(midTileEdge + c);
                if (i == 8)
                {
                    Console.WriteLine(midTileEdge + (9 - x));
                    Console.Write(tileEdge + (large ? "\n" : ""));
                    Console.WriteLine(rowSplit);
                    Console.Write(large ? (x == 8 ? "" : tileEdge) + "\n" : "");
                    i = 0;
                    x++;
                }
            }
            Console.WriteLine(letterCords + "\n");
            Console.WriteLine(line);
            Console.WriteLine("Evaluation: " + Functions.Eval(board));
            Console.WriteLine("FEN: " + Functions.GenerateFEN(board, formatFEN));
            Console.WriteLine(line);
        }
        public static string[] StartDialog()
        {
            Console.Clear();
            while (true)
            {
                Console.Write("Do you want large graphics ? [yes/no]: ");
                string input = "" +  Console.ReadLine();
                if (input == "yes" || input == "no")
                {
                    large = input == "yes";
                    break;
                }
            }
            Console.Clear();
            while (true)
            {
                Console.Write("Do you want start game from a custom FEN ? [yes/no]: ");
                string input = "" + Console.ReadLine();
                if (input == "no")
                    break;
                else if(input == "yes")
                {
                    Console.Clear();
                    while(true)
                    {
                        Console.Write("Enter full FEN to start the game: ");
                        string inputFEN = "" + Console.ReadLine();
                        string[] formatFEN = inputFEN.Split(" ");
                        if (inputFEN.Count(f => f == '/') == 7 && formatFEN.Length == 6)
                            return formatFEN;
                    }
                }
            }
            Console.Clear();
            return mainFEN.Split(" ");
        }
        public static string PlayAI()
        {
            while (true)
            {
                Console.Write("Do you want to play against a computer ? [yes/no]: ");
                string input = "" + Console.ReadLine();
                if(input == "no")
                    return input;
                else if (input == "yes")
                    while(true)
                    {
                        Console.Write("Choose your color [w - white / b - black / r - random]: ");
                        string color = "" + Console.ReadLine();
                        if(color == "w" || color == "b" || color == "r")
                            return color;
                    }
            }
        }
    }
}