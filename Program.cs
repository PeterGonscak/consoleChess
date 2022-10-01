﻿
using System;
using System.Linq;
using System.Collections.Generic;

namespace test
{
    static class Program
    {
        static readonly string line = "________________________________________________________________________________________________\n";
        static string mainFEN = "RNBQKBNR/PPPPPPPP/8/8/8/8/pppppppp/rnbqkbnr w kqKQ 0 0";          // test psoition 2b3kr/2p1qppp/r1n5/2p5/pb1pB1RP/N4N2/PP1BQPP1/2R3K1 w 0 0
        static readonly Dictionary<char, int> pieceValues = new Dictionary<char, int>(){                    //   constant values
            {'p', 1}, {'P', -1},
            {'n', 3}, {'N', -3},
            {'b', 3}, {'B', -3},
            {'r', 5}, {'R', -5},
            {'q', 9}, {'Q', -9},
            {'k', 0}, {'K', 0},
            {' ', 0}
        };
        static readonly Dictionary<char, int> tileValues = new Dictionary<char, int>(){                   //
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
        static readonly int[][] nMoves = new int[4][] { 
            new int[2] { -10, 6 }, 
            new int[2] { -17, 15 }, 
            new int[2] { -6, 10 }, 
            new int[2] { -15, 17 } 
        };
        static readonly int[] bMoves = new int[4] { -9, -7, 7, 9};
        static readonly int[] rMoves = new int[4] {-8, -1, 1, 8 };
        static readonly int[] qkMoves = new int[8] { -9, -7, 7, 9, -8, -1, 1, 8 };
        static readonly string[] bw = new string[] { "w", "b" };
        static void Main()
        {
            Console.Clear();
            Console.Write(line + "\ntype 'yes' for large version, other input is default gameboard: ");
            bool large = Console.ReadLine() == "yes";
            Console.Clear();
            Console.Write(line + "\nEnter FEN for game in progress, press ENTER to generate new game: ");
            string input = Console.ReadLine();                                                             //
            if (input != null)                                                                            //
                if (input.ToCharArray().Count(f => f == ('/')) == 7)                                     //     Input for starting position
                    mainFEN = input;                                                                    //
                else                                                                                   //
                    Console.WriteLine("Wrong Input");                                                 //
            Thread.Sleep(1000);
            Console.Clear();
            string[] formatFEN = mainFEN.Split(" ");   //
            List<char> board = new List<char>();      //    board generated by FEN
            board = GenerateBoard(formatFEN[0]);     //
            bool[] checks = new bool[64];
            bool gameOn = true;
            while (gameOn)
            {
                Console.Clear();
                Console.WriteLine(line);
                Console.WriteLine("Play by writing move in format: XX(from)XX(to) - ex: d4e5");
                Console.WriteLine("For resignation write 'resign'");
                Console.WriteLine(line);
                WriteBoard(board, large, formatFEN);
                checks = checkChecker(board, formatFEN[1]);
                Console.WriteLine(formatFEN[1] + " to move.");
                while (true)
                {
                    Console.Write("Enter a valid move: ");
                    string move = Console.ReadLine();
                    if (move == "resign")
                    {
                        gameOn = false;
                        break;
                    }
                    try
                    {
                        if (MakeMove(move, board, formatFEN[1], checks))
                        {
                            formatFEN[1] = bw[1 - Array.IndexOf(bw, formatFEN[1])];
                            break;
                        }
                    }
                    catch { }
                }
            }
        }
        static bool MakeMove(string s, List<char> board, string onTurn, bool[] checks)
        {
            int sPos = TileToNum(s.Substring(0, 2));
            int ePos = TileToNum(s.Substring(2, 2));
            char[] testBoard = board.ToArray();
            testBoard[ePos] = testBoard[sPos];
            testBoard[sPos] = ' ';
            if (((onTurn == "w" && char.IsLower(board[sPos]) && !checkChecker(testBoard.ToList(), onTurn)[Array.IndexOf(testBoard, 'k')]) 
              || (onTurn == "b" && char.IsUpper(board[sPos]) && !checkChecker(testBoard.ToList(), onTurn)[Array.IndexOf(testBoard, 'K')]))
            && IsValid(sPos, ePos, board, checks))
            {
                board[ePos] = board[sPos];
                board[sPos] = ' ';
                return true;
            }
            return false;
        }
        static bool IsValid(int sPos, int ePos, List<char> board, bool[] checkBoard)
        {
            if (sPos == ePos)
                return false;
            if (board[sPos] == 'p')
                return (((sPos == ePos + 8 || (sPos == ePos + 16 && sPos > 47))                              // 1 or 2 tiles forward if on starting tile 
                    && board[ePos] == ' ' && (board[ePos + 8] == ' ' || !(sPos == ePos + 16)))          // if tiles in front are empty 
                || (((sPos == ePos + 7 && (sPos + 1) % 8 != 0)                   // take to side if not on edge of the board
                    || (sPos == ePos + 9 && sPos % 8 != 0))                     // other side
                        && char.IsUpper(board[ePos])));                         // it is enemy piece (or pawn)
            else if (board[sPos] == 'P')                                       // same thing for black
            {
                return (((sPos == ePos - 8 || (sPos == ePos - 16 && sPos < 16))
                    && board[ePos] == ' ' && (board[ePos - 8] == ' ' || !(sPos == ePos - 16)))
                || (((sPos == ePos - 7 && sPos % 8 != 0)
                    || (sPos == ePos - 9 && (sPos + 1) % 8 != 0))
                        && char.IsLower(board[ePos])));
            }
            else
                switch (char.ToLower(board[sPos]))
                {
                    case 'n':
                        return ((((nMoves[0].Contains(ePos - sPos) || nMoves[1].Contains(ePos - sPos)) && sPos % 8 != 0)
                            || (nMoves[0].Contains(ePos - sPos) && (sPos - 1) % 8 != 0)
                            || ((nMoves[2].Contains(ePos - sPos) || nMoves[3].Contains(ePos - sPos)) && (sPos + 1) % 8 != 0)
                            || (nMoves[2].Contains(ePos - sPos) && (sPos + 2) % 8 != 0))
                            && ((char.IsLower(board[sPos]) && !char.IsLower(board[ePos]))
                                || (char.IsUpper(board[sPos]) && !char.IsUpper(board[ePos]))));
                    case 'b':
                        foreach (int d in bMoves)
                        {
                            int testTile = sPos;
                            while ((!(testTile % 8 == 0 && (d == bMoves[0] || d == bMoves[2])))
                            && (!(testTile % 8 == 7 && (d == bMoves[1] || d == bMoves[3])))
                            && (!(testTile < 8 && (d == bMoves[0] || d == bMoves[1])))
                            && (!(testTile > 55 && (d == bMoves[2] || d == bMoves[3]))))
                            {
                                testTile += d;
                                if ((char.IsLower(board[sPos]) && char.IsLower(board[testTile]))
                                || (char.IsUpper(board[sPos]) && char.IsUpper(board[testTile]))
                                || (board[testTile] != ' ' && testTile != ePos))
                                    break;
                                if (testTile == ePos)
                                    return true;
                            }
                        }
                        return false;
                    case 'r':
                        foreach (int d in rMoves)
                        {
                            int testTile = sPos;
                            while ((!(testTile % 8 == 0 && d == rMoves[1]))
                            && (!(testTile % 8 == 7 && d == rMoves[2]))
                            && (!(testTile < 8 && d == rMoves[0]))
                            && (!(testTile > 55 && d == rMoves[3])))
                            {
                                testTile += d;
                                if ((char.IsLower(board[sPos]) && char.IsLower(board[testTile]))
                                || (char.IsUpper(board[sPos]) && char.IsUpper(board[testTile]))
                                || (board[testTile] != ' ' && testTile != ePos))
                                    break;
                                if (testTile == ePos)
                                    return true;
                            }
                        }
                        return false;
                    case 'q':
                        foreach (int d in qkMoves)
                        {
                            int testTile = sPos;
                            while ((!(testTile % 8 == 0 && (d == qkMoves[0] || d == qkMoves[2] || d == qkMoves[5])))
                            && (!(testTile % 8 == 7 && (d == qkMoves[1] || d == qkMoves[3] || d == qkMoves[6])))
                            && (!(testTile < 8 && (d == qkMoves[0] || d == qkMoves[1] || d == qkMoves[4])))
                            && (!(testTile > 55 && (d == qkMoves[2] || d == qkMoves[3] || d == qkMoves[7])))) {
                                testTile += d;
                                if ((char.IsLower(board[sPos]) && char.IsLower(board[testTile]))
                                || (char.IsUpper(board[sPos]) && char.IsUpper(board[testTile]))
                                || (board[testTile] != ' ' && testTile != ePos))
                                    break;
                                if (testTile == ePos)
                                    return true;
                            }
                        }
                        return false;
                   case 'k':
                        if((!(sPos % 8 == 0 && (qkMoves[0] == (ePos-sPos)
                                            || qkMoves[2] == (ePos-sPos)
                                            || qkMoves[5] == (ePos-sPos))))
                        && !(sPos % 8 == 7 && (qkMoves[1] == (ePos-sPos)
                                            || qkMoves[3] == (ePos-sPos)
                                            || qkMoves[6] == (ePos-sPos)))
                        && qkMoves.Contains(ePos-sPos) && !checkBoard[ePos]
                        && ((char.IsLower(board[sPos]) && !char.IsLower(board[ePos]))
                        || (char.IsUpper(board[sPos]) && !char.IsUpper(board[ePos]))))
                            return true;
                        return false;
                }
                return false;
        }
       static bool[] checkChecker(List<char> boardA, string onTurn)
        {
            char[] board = boardA.ToArray();
            bool[] checks = new bool[64];
            for(int i = 0; i < 64; i++){
                if((onTurn == "w" && char.IsLower(board[i])) 
                || (onTurn == "b" && char.IsUpper(board[i])))
                    board[i] = ' ';
                if(board[i] == ' ')
                    continue;
                if(board[i] == 'p'){
                    if(i % 8 != 0)
                        checks[i-9] = true;
                    if(i % 8 != 7)
                        checks[i-7] = true;
                }
                else if (board[i] == 'P'){
                    if(i % 8 != 0)
                        checks[i+7] = true;     
                    if(i % 8 != 7)
                        checks[i+9] = true;
                }                
                else
                    switch(char.ToLower(board[i]))
                    {    
                        case 'n':
                            if(i % 8 != 0){
                                if(i % 8 != 1){
                                    if(i > 7)
                                        checks[i+nMoves[0][0]] = true;
                                    if(i < 56)
                                        checks[i+nMoves[0][1]] = true;
                                }
                                if(i > 15)
                                    checks[i+nMoves[1][0]] = true;
                                if(i < 48)
                                    checks[i+nMoves[1][1]] = true;
                            }
                            if(i % 8 != 7){
                                if(i % 8 != 6){
                                    if(i > 7)
                                        checks[i+nMoves[2][0]] = true;
                                    if(i < 54)
                                        checks[i+nMoves[2][1]] = true;
                                }
                                if(i > 15)
                                    checks[i+nMoves[3][0]] = true;
                                if(i < 48)
                                    checks[i+nMoves[3][1]] = true;
                            }
                            break;
                        case 'b':
                            foreach (int d in bMoves){
                               int testTile = i;
                                while ((!(testTile % 8 == 0 && (d == bMoves[0] || d == bMoves[2])))
                                && (!(testTile % 8 == 7 && (d == bMoves[1] || d == bMoves[3])))
                                && (!(testTile < 8 && (d == bMoves[0] || d == bMoves[1])))
                                && (!(testTile > 55 && (d == bMoves[2] || d == bMoves[3])))){
                                    testTile += d;
                                    checks[testTile] = true;
                                    if(boardA[testTile] != ' ')
                                        break;
                                }
                            }
                            break;
                        case 'r':
                            foreach (int d in rMoves){
                                int testTile = i;
                                while ((!(testTile % 8 == 0 && d == rMoves[1]))
                                && (!(testTile % 8 == 7 && d == rMoves[2]))
                                && (!(testTile < 8 && d == rMoves[0]))
                                && (!(testTile > 55 && d == rMoves[3]))){
                                    testTile += d;
                                    checks[testTile] = true;
                                    if(boardA[testTile] != ' ')
                                        break;
                                }
                            }
                            break;
                        case 'q':
                            foreach (int d in qkMoves){
                                int testTile = i;
                                while ((!(testTile % 8 == 0 && (d == qkMoves[0] || d == qkMoves[2] || d == qkMoves[5])))
                                && (!(testTile % 8 == 7 && (d == qkMoves[1] || d == qkMoves[3] || d == qkMoves[6])))
                                && (!(testTile < 8 && (d == qkMoves[0] || d == qkMoves[1] || d == qkMoves[4])))
                                && (!(testTile > 55 && (d == qkMoves[2] || d == qkMoves[3] || d == qkMoves[7])))) {
                                    testTile += d;
                                    checks[testTile] = true;
                                    if(boardA[testTile] != ' ')
                                        break;
                                }
                            }
                            break;
                        case 'k':
                            foreach(int d in qkMoves){
                                if((!(i % 8 == 0 && (d == qkMoves[0] || d == qkMoves[2] || d == qkMoves[5])))
                                && (!(i % 8 == 7 && (d == qkMoves[1] || d == qkMoves[3] || d == qkMoves[6])))
                                && (!(i < 8 && (d == qkMoves[0] || d == qkMoves[1] || d == qkMoves[4])))
                                && (!(i > 55 && (d == qkMoves[2] || d == qkMoves[3] || d == qkMoves[7]))))
                                    checks[i+d] = true;
                            }
                            break;
                    }
            }
            return checks;
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
        static void WriteBoard(List<char> board, bool large, string[] formatFEN)
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
            Console.WriteLine("\n" + letterCords + (large ? "\n" : ""));
            Console.WriteLine(rowSplit);
            Console.Write(tileEdge + (large ? "\n" : ""));
            foreach (char c in board)
            {
                if (i == 0)
                    Console.Write("    " + (9 - x));
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
            Console.WriteLine("Evaluation: " + Eval(board));
            Console.WriteLine("FEN: " + GenerateFEN(board, formatFEN));
            Console.WriteLine(line);
        }
        static void WriteChecks(bool[] board, bool large)
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
            Console.WriteLine("\n" + letterCords + (large ? "\n" : ""));
            Console.WriteLine(rowSplit);
            Console.Write(tileEdge + (large ? "\n" : ""));
            foreach (bool c in board)
            {
                if (i == 0)
                    Console.Write("    " + (9 - x));
                i++;
                Console.Write(midTileEdge + (c ? "X" : " "));
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
        }
        static int Eval(List<char> board)
        {
            int sum = 0;
            foreach (char c in board)
                sum += pieceValues[c];
            return sum;
        }
        static string GenerateFEN(List<char> board, string[] formatFEN)
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
                    if (tileCount == 0)
                        FEN += "/";
                    else
                    {
                        FEN += tileCount + "/";
                        tileCount = 0;
                    }
            }
            FEN += " " + formatFEN[1] + " " + formatFEN[2] + " " + formatFEN[3];
            return FEN;
        }
    }
}
