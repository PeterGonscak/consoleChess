using System;

namespace test
{
    public static class Graphics
    {
        public static int i = 0;
        public static int x = 1;
        public static string rowSplit = "      -|-----|-----|-----|-----|-----|-----|-----|-----|-";
        public static string letterCords = "          A     B     C     D     E     F     G     H";
        public static string midTileEdge = "  |  ";
        public static string tileEdge = "";
        public static string rowSplitLarge = "        -|---------|---------|---------|---------|---------|---------|---------|---------|-";
        public static string letterCordsLarge = "              A         B         C         D         E         F         G         H";
        public static string midTileEdgeLarge = "    |    ";
        public static string tileEdgeLarge = "         |         |         |         |         |         |         |         |         | ";
        public static string line = "________________________________________________________________________________________________\n";
        
        public static void WriteHint()
        {
            Console.WriteLine(line);
            Console.WriteLine("Play by writing move in format: XX(from)XX(to) - ex: d4e5");
            Console.WriteLine("For resignation write 'resign'");
            Console.WriteLine(line);  
        }
        public static void WriteBoard(List<char> board, bool large, string[] formatFEN)
        {
            int i = 0;
            int x = 1;
            string rowSplit = "      -|-----|-----|-----|-----|-----|-----|-----|-----|-";
            string letterCords = "          A     B     C     D     E     F     G     H";
            string midTileEdge = "  |  ";
            string tileEdge = "";
            if (large)
            {
                rowSplit = "        -|---------|---------|---------|---------|---------|---------|---------|---------|-";
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
    }
}