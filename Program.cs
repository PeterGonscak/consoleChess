﻿using System;
using System.Linq;
using System.Collections.Generic;

namespace test
{
    static class Program
    {
        static readonly string line = "________________________________________________________________________________________________\n";
        static string mainFEN = "RNBQKBNR/PPPPPPPP/8/8/8/8/pppppppp/rnbqkbnr w kqKQ 0 0";          // test psoition 2b3kr/2p1qppp/r1n5/2p5/pb1pB1RP/N4N2/PP1BQPP1/2R3K1 w 0 0
        static readonly Dictionary<char, int> pieceValues = new Dictionary<char, int>(){                    //   constant values
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
        static readonly int[][] nMoves = new int[4][]{new int[2]{-10, 6},new int[2]{-17, 15},new int[2]{-6, 10},new int[2]{-15, 17}};
        static readonly int[] moves = new int[8]{-9, -7, 7, 9, -8, -1, 1, 8};
        static readonly string[] bw = new string[] { "w", "b" };
        static void Main()
        {
            Console.Write(line + "\ntype 'yes' for large version, other input is default gameboard: ");
            bool large = Console.ReadLine() == "yes";
            Console.Write(line + "\nEnter FEN for game in progress, press ENTER to generate new game: ");             //
            string input = Console.ReadLine();                                                             //
            if (input != null)                                                                              //
                if (input.ToCharArray().Count(f => f == ('/')) == 7)                                     //     Input for starting position
                    mainFEN = input;                                                                    //
                else                                                                                   //
                    Console.WriteLine("Wrong Input");                                                 //

            string[] formatFEN = mainFEN.Split(" ");   //
            List<char> board = new List<char>();      //    board generated by FEN
            board = GenerateBoard(formatFEN[0]);     //

            Console.WriteLine(line);
            Console.WriteLine("Play by writing move in format: XX(from)XX(to) - ex: d4e5");
            Console.WriteLine("For resignation write 'resign'");
            Console.WriteLine(line);
            bool gameOn = true;
            while (gameOn) {
                WriteBoard(board,large);
                Console.WriteLine(formatFEN[1] + " to move.");
                while (true) {                                               //
                    Console.Write("Enter an valid move: ");                 //     loop for valid move
                    string move = Console.ReadLine();                      //
                    if(move == "resign") {
                        gameOn = false;
                        break;
                    }
                    try {
                        if (MakeMove(move, board, char.Parse(formatFEN[1]))) {
                            formatFEN[1] = bw[1 - Array.IndexOf(bw, formatFEN[1])];
                            break;
                        }
                    } catch {}
                    Console.WriteLine(line);
                }
            }
        }

        static bool MakeMove(string s, List<char> board, char onTurn)
        {
            int sPos = TileToNum(s.Substring(0, 2));
            int ePos = TileToNum(s.Substring(2, 2));
            if (((onTurn == 'w' && char.IsLower(board[sPos])) || (onTurn == 'b' && char.IsUpper(board[sPos]))) && IsValid(sPos, ePos, board)) {
                board[ePos] = board[sPos];
                board[sPos] = ' ';
                return true;
            }
            return false;
        }

        static bool IsValid(int sPos,int ePos, List<char> board)
        {
            if(board[sPos] == 'p')
                return (((sPos == ePos + 8 || (sPos == ePos +16 && sPos > 47))                              // 1 or 2 tiles forward if on starting tile 
                    && board[ePos] == ' ' && (board[ePos+8] == ' ' || !(sPos == ePos +16)))          // if tiles in front are empty 
                || (((sPos == ePos + 7 && (sPos + 1) % 8 != 0)                   // take to side if not on edge of the board
                    || (sPos == ePos + 9 && sPos % 8 != 0))                     // other side
                        && char.IsUpper(board[ePos])));                         // it is enemy piece (or pawn)
            else if (board[sPos] == 'P')                                       // same thing for black
                return (((sPos == ePos - 8 || (sPos == ePos - 16 && sPos < 16))                     
                    && board[ePos] == ' ' && (board[ePos-8] == ' ' || !(sPos == ePos -16)))  
                || (((sPos == ePos - 7 && sPos % 8 != 0) 
                    || (sPos == ePos - 9 && (sPos+ 1) % 8 != 0)) 
                        &&  char.IsLower(board[ePos])));
            else
                switch (char.ToLower(board[sPos])){
                    case 'n':
                        return ((((nMoves[0].Contains(ePos-sPos) || nMoves[1].Contains(ePos-sPos)) && sPos % 8 != 0)
                            || (nMoves[0].Contains(ePos-sPos) && (sPos - 1) % 8 != 0)
                            || ((nMoves[2].Contains(ePos-sPos) || nMoves[3].Contains(ePos-sPos)) && (sPos+1) % 8 != 0)
                            || (nMoves[2].Contains(ePos-sPos) && (sPos + 2) % 8 != 0))
                            && ((char.IsLower(board[sPos]) && !char.IsLower(board[ePos])) 
                                || (char.IsUpper(board[sPos]) && !char.IsUpper(board[ePos]))));
                    case 'b':
                        if(sPos != ePos)
                            foreach(int d in moves.){
                                int testTile = sPos;
                                while((!(testTile % 8 == 0 && (d == moves[0] || d == moves[2])))
                                && (!(testTile % 8 == 7 && (d == moves[1] || d == moves[3])))
                                && (!(testTile < 8 && (d == moves[0] || d == moves[1])))
                                && (!(testTile > 55 && (d == moves[2] || d == moves[3])))){
                                    testTile += d;   
                                    if ((char.IsLower(board[sPos]) && char.IsLower(board[testTile])) 
                                    || (char.IsUpper(board[sPos]) && char.IsUpper(board[testTile]))
                                    || (board[testTile] != ' ' && testTile != ePos))
                                        break;
                                    if(testTile == ePos)
                                        return true;
                            }
                        }
                        return false;
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

        static void WriteBoard(List<char> board, bool large)
        {
            int i = 0;
            int x = 1;
            string rowSplit = "      -|-----|-----|-----|-----|-----|-----|-----|-----|-";
            string letterCords = "          A     B     C     D     E     F     G     H";
            string midTileEdge = "  |  ";
            string tileEdge = "";
            if(large) {
                rowSplit ="        -|---------|---------|---------|---------|---------|---------|---------|---------|-";
                letterCords = "              A         B         C         D         E         F         G         H";
                midTileEdge = "    |    ";
                tileEdge = "         |         |         |         |         |         |         |         |         | ";
            }
            Console.WriteLine("\n" + letterCords + (large ? "\n" : ""));
            Console.WriteLine(rowSplit);
            Console.Write(tileEdge + (large ? "\n" : ""));
            foreach (char c in board) {
                if (i == 0)
                    Console.Write("    " + (9 - x));
                i++;
                Console.Write(midTileEdge + c);
                if (i == 8) {
                    Console.WriteLine(midTileEdge + (9 - x));
                    Console.Write(tileEdge + (large ? "\n" : ""));
                    Console.WriteLine(rowSplit);
                    Console.Write(large ? (x==8 ? "" : tileEdge)+ "\n" : "");
                    i = 0;
                    x++;
                }
            }
            Console.WriteLine(letterCords + "\n");
            Console.WriteLine(line);
            Console.WriteLine("Evaluation: " + Eval(board));
            Console.WriteLine("FEN: " + GenerateFEN(board));
            Console.WriteLine(line);
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
            for (int i = 0; i < board.Count; i++) {
                if (board[i] == ' ')
                    tileCount++;
                else if (tileCount == 0)
                    FEN += board[i];
                else {
                    FEN += tileCount.ToString() + board[i];
                    tileCount = 0;
                }
                if ((i + 1) % 8 == 0 && (i + 1) != 64)
                    if (tileCount == 0)
                        FEN += "/";
                    else {
                        FEN += tileCount + "/";
                        tileCount = 0;
                    }
            }
            return FEN;
        }
    }
}
