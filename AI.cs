using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading;

namespace test
{
    public class AI
    {
        static readonly int[][] distanceToEdge = Functions.DistanceToEdge();
        readonly bool[,] knightLegalMoves = Functions.LegalKnightMoves(distanceToEdge);
        readonly int depth = 5;
        readonly int[] bpMoves = new int[] { 7, 8, 9, 16 };
        readonly int[] wpMoves = new int[] { -7, -8, -9, -16 };
        readonly int[] nMoves = new int[] { -17, -15, -6, 10, 17, 15, 6, -10 };
        readonly int[] bMoves = new int[4] { -9, -7, 9, 7 };
        readonly int[] rMoves = new int[4] { -8, 1, 8, -1 };
        readonly int[] qkMoves = new int[8] { -9, -7, 9, 7, -8, 1, 8, -1 };

        readonly int[][] pTiles = new int[][]{
            new int[]{27, 28, 34, 35, 36, 37},
            new int[]{33, 26, 19, 20, 29, 38, 42, 43, 44, 45}
        };
        readonly int[][] nTiles = new int[][]{
            new int[]{27, 28, 35, 36},
            new int[]{19, 20, 26, 29, 34, 37, 43, 44},
            new int[]{11, 12, 21, 30, 38, 45, 52, 51, 42, 33, 25, 18},
        };
        readonly int[][] bTiles = new int[][]{
            new int[]{41, 42, 49, 50, 45, 46, 53, 54},
            new int[]{33, 34, 37, 38},
            new int[]{40, 48, 47, 55}
        };
        readonly int[][] rTiles = new int[][] {
            new int[]{58, 59, 60, 61},
            new int[]{57, 62}
        };
        readonly int[][] qTiles = new int[][]{
            new int[]{41, 42, 45, 46, 49, 50, 51, 52, 53, 54},
            new int[]{33, 34, 35, 36, 37, 38, 43, 44},
        };
        readonly int[][] kTiles = new int[][]{
            new int[]{56, 57, 62, 63},
            new int[]{48, 49, 58, 61, 54, 55}
        };

        readonly Dictionary<int, double[]> valueWeights = new Dictionary<int, double[]>(){
            {0, new double[]{0.2, 0.1}},
            {1, new double[]{0.7, 0.3, 0.1}},
            {2, new double[]{0.7, 0.5, 0.1}},
            {3, new double[]{0.5, 0.2}},
            {4, new double[]{0.5, 0.2}},
            {5, new double[]{0.7, 0.3}}
        };
        readonly Dictionary<int, char> numToPiece = new Dictionary<int, char>()
        {
            {0, ' '},
            {1, 'p'},
            {2, 'n'},
            {3, 'b'},
            {4, 'r'},
            {5, 'q'},
            {6, 'k'},
            {7, 'P'},
            {8, 'N'},
            {9, 'B'},
            {10, 'R'},
            {11, 'Q'},
            {12, 'K'}
        };
        readonly Dictionary<char, int> pieceToNum = new Dictionary<char, int>()
        {

            {'p', 0},
            {'n', 1},
            {'b', 2},
            {'r', 3},
            {'q', 4},
            {'k', 5},
            {' ', 6},
            {'P', 7},
            {'N', 8},
            {'B', 9},
            {'R', 10},
            {'Q', 11},
            {'K', 12}
        };
        readonly Dictionary<int, int> pieceValues = new Dictionary<int, int>(){
            {0, 1}, {7, -1},
            {1, 3}, {8, -3},
            {2, 3}, {9, -3},
            {3, 5}, {10, -5},
            {4, 9}, {11, -9},
            {5, 1}, {12, -1}
        };
        public AI(int depth)
        {
            this.depth = depth;
        }
        public Dictionary<int[], int> moves = new Dictionary<int[], int>();
        int checkCounter = 0;
        int enPc = 0;
        List<bool[]> iHateMyLife = new List<bool[]>();

        public void PlayMove(List<char> position, string[] FENstring)
        {
            List<int> board = ConvertList(position);
            board.Add(FENstring[1] == "w" ? 0 : 1);
            board.Add(FENstring[2].Contains('k') ? 1 : 0);
            board.Add(FENstring[2].Contains('q') ? 1 : 0);
            board.Add(FENstring[2].Contains('K') ? 1 : 0);
            board.Add(FENstring[2].Contains('Q') ? 1 : 0);
            board.Add(FENstring[3] != "-" ? Functions.TileToNum(FENstring[3]) : -999);
            Graphics.WriteBoard(position, FENstring);
            // bool[] checks = CheckChecker(board.ToArray());
            //         for(int i = 0; i < 64; i++)
            //         {
            //             if(i % 8 == 0)
            //                 Console.WriteLine("");
            //             Console.Write(checks[i] ? " X "  : "   ");
            //         }
            //         System.Console.Write("\n "+ board.Count() + " ");
            //         for(int i = 0; i < board.Count(); i++)
            //         {
            //             Console.Write(board[i] + " ");
            //             if(i % 8 == 0)
            //                 Console.WriteLine("");
            //         }
            Stopwatch timer = new Stopwatch();
            timer.Start();
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine((i + 1) + ". move: " + FindAllMoves(board.ToArray(), i) + $"  Time: {timer.ElapsedMilliseconds} ms.");
            }
            timer.Stop();
            Console.WriteLine(checkCounter);
            Console.WriteLine(enPc);
            foreach(var item in iHateMyLife)
                System.Console.WriteLine(item[0] + " " + item[1]);
            System.Console.WriteLine(iHateMyLife.Count());
        }
        public int FindAllMoves(int[] board, int depth)
        {
            int counter = 0;
            if (board[64] == 0)                                                                                     //white
            {
                int kingTile = Array.IndexOf(board, 5);
                for (int i = 0; i < 64; i++)
                {
                    if (board[i] > 5)
                        continue;
                    switch (board[i])
                    {
                        case 0:                                                                                    //pawn
                            if(!(board[i + 1] == board[69]
                                && distanceToEdge[i][5] != 0))
                                    iHateMyLife.Add(new bool[] {board[i + 1] == board[69], distanceToEdge[i][5] != 0} );
                            if (i > 47 && board[i - 16] == 6 && board[i - 8] == 6)
                            {
                                int[] testBoard = MakeMove(i, i - 16, board);
                                if (CheckChecker(testBoard)[kingTile])
                                    continue;
                                if (depth != 0)
                                {
                                    counter += FindAllMoves(testBoard, depth - 1);
                                }
                                else
                                    counter++;
                            }
                            if (board[i - 8] == 6)
                            {
                                int[] testBoard = MakeMove(i, i - 8, board);
                                if (CheckChecker(testBoard)[kingTile])
                                    continue;
                                if (depth != 0)
                                {
                                    counter += FindAllMoves(testBoard, depth - 1);
                                }
                                else
                                    counter++;
                            }
                            if ((board[i - 9] > 6 || board[i - 1] == board[69])
                                && distanceToEdge[i][7] != 0)
                            {
                                if(board[i - 1] == board[69]
                                && distanceToEdge[i][7] != 0)
                                    checkCounter++;
                                int[] testBoard = MakeMove(i, i - 9, board);
                                if (CheckChecker(testBoard)[kingTile])
                                    continue;
                                if (depth != 0)
                                {
                                    counter += FindAllMoves(testBoard, depth - 1);
                                }
                                else
                                    counter++;
                            }
                            if ((board[i - 7] > 6 || board[i + 1] == board[69])
                                && distanceToEdge[i][5] != 0)
                            {
                                if(board[i + 1] == board[69]
                                && distanceToEdge[i][5] != 0)
                                    checkCounter++;
                                int[] testBoard = MakeMove(i, i - 7, board);
                                if (CheckChecker(testBoard)[kingTile])
                                    continue;
                                if (depth != 0)
                                {
                                    counter += FindAllMoves(testBoard, depth - 1);
                                }
                                else
                                    counter++;
                            }
                            break;
                        case 1:                                                                                     //knight
                            for (int x = 0; x < 8; x++)
                                if (knightLegalMoves[i, x] && board[i + nMoves[x]] > 5)
                                {
                                    int[] testBoard = MakeMove(i, i + nMoves[x], board);
                                    if (CheckChecker(testBoard)[kingTile])
                                        continue;
                                    if (depth != 0)
                                    {
                                        counter += FindAllMoves(testBoard, depth - 1);
                                    }
                                    else
                                        counter++;
                                    //allPositions[0].Add(new int[] {board[i], i, i + nMoves[x]});
                                    //allPositions[1].Add(testBoard);
                                }
                            break;
                        case 2:                                                                                     //bishop
                            for (int x = 0; x < 4; x++)
                            {
                                int testTile = i;
                                for (int y = 0; y < distanceToEdge[i][x]; y++)
                                {
                                    testTile += bMoves[x];
                                    if (board[testTile] < 6)
                                        break;
                                    int[] testBoard = MakeMove(i, i + bMoves[x], board);
                                    if (CheckChecker(testBoard)[kingTile])
                                        continue;
                                    if (depth != 0)
                                    {
                                        counter += FindAllMoves(testBoard, depth - 1);
                                    }
                                    else
                                        counter++;
                                    //allPositions[0].Add(new int[] {board[i], i, i + bMoves[x]});
                                    //allPositions[1].Add(testBoard);
                                    if (board[testTile] != 6)
                                        break;
                                }
                            }
                            break;
                        case 3:                                                                                     //rook
                            for (int x = 4; x < 8; x++)
                            {
                                int testTile = i;
                                for (int y = 0; y < distanceToEdge[i][x]; y++)
                                {
                                    testTile += rMoves[x - 4];
                                    if (board[testTile] < 6)
                                        break;
                                    int[] testBoard = MakeMove(i, i + rMoves[x - 4], board);
                                    if (CheckChecker(testBoard)[kingTile])
                                        continue;
                                    if (depth != 0)
                                    {
                                        counter += FindAllMoves(testBoard, depth - 1);
                                    }
                                    else
                                        counter++;
                                    //allPositions[0].Add(new int[] {board[i], i, i + rMoves[x - 4]});
                                    //allPositions[1].Add(testBoard);
                                    if (board[testTile] != 6)
                                        break;
                                }
                            }
                            break;
                        case 4:                                                                                     //queen
                            for (int x = 0; x < 8; x++)
                            {
                                int testTile = i;
                                for (int y = 0; y < distanceToEdge[i][x]; y++)
                                {
                                    testTile += qkMoves[x];
                                    if (board[testTile] < 6)
                                        break;
                                    int[] testBoard = MakeMove(i, i + qkMoves[x], board);
                                    if (CheckChecker(testBoard)[kingTile])
                                        continue;
                                    if (depth != 0)
                                    {
                                        counter += FindAllMoves(testBoard, depth - 1);
                                    }
                                    else
                                        counter++;
                                    //allPositions[0].Add(new int[] {board[i], i, i + qkMoves[x]});
                                    //allPositions[1].Add(testBoard);
                                    if (board[testTile] != 6)
                                        break;
                                }
                            }
                            break;
                        case 5:                                                                                     //king
                            foreach (int move in qkMoves)
                            {
                                if (i + move < 0 || i + move > 63 || board[i + move] < 6)
                                    continue;
                                int[] testBoard = MakeMove(i, i + move, board);
                                if (CheckChecker(testBoard)[i + move])
                                    continue;
                                if (depth != 0)
                                {
                                    counter += FindAllMoves(testBoard, depth - 1);
                                                            }
                                else
                                    counter++;
                            }
                            bool[] checkBoard = CheckChecker(board);
                            if (!checkBoard[60])
                            {
                                if (board[66] == 1 && (!checkBoard[59]) && (!checkBoard[58])
                                && board[59] == 6 && board[58] == 6)
                                {
                                    int[] testBoard = MakeMove(60, 58, board);
                                    if (depth != 0)
                                    {
                                        counter += FindAllMoves(testBoard, depth - 1);
                                    }
                                    else
                                        counter++;
                                    //allPositions[0].Add(new int[] {board[i], 60, 58});
                                    //allPositions[1].Add(testBoard);
                                }
                                if (board[65] == 1 && (!checkBoard[61]) && (!checkBoard[62])
                                && board[61] == 6 && board[62] == 6)
                                {
                                    int[] testBoard = MakeMove(60, 62, board);
                                    if (depth != 0)
                                    {
                                        counter += FindAllMoves(testBoard, depth - 1);
                                    }
                                    else
                                        counter++;
                                    //allPositions[0].Add(new int[] {board[i], 60, 62});
                                    //allPositions[1].Add(testBoard);
                                }
                            }
                            break;
                    }
                }
            }
            else                                                                                                //black
            {
                int  kingTile = Array.IndexOf(board, 12);
                for (int i = 0; i < 64; i++)
                {
                    if (board[i] < 7)
                        continue;
                    switch (board[i])
                    {
                        case 7:                                                                                     //pawn
                            if (i < 16 && board[i + 16] == 6 && board[i + 8] == 6)
                            {
                                int[] testBoard = MakeMove(i, i + 16, board);
                                if (CheckChecker(testBoard)[kingTile])
                                    continue;
                                if (depth != 0)
                                {
                                    counter += FindAllMoves(testBoard, depth - 1);
                                                            }
                                else
                                    counter++;
                            }
                            if (board[i + 8] == 6)
                            {
                                int[] testBoard = MakeMove(i, i + 8, board);
                                if (CheckChecker(testBoard)[kingTile])
                                    continue;
                                if (depth != 0)
                                {
                                    counter += FindAllMoves(testBoard, depth - 1);
                                                            }
                                else
                                    counter++;
                            }
                            if ((board[i + 9] < 6 || board[i + 1] == board[69])
                                && distanceToEdge[i][5] != 0)
                            {
                                if(board[i + 1] == board[69]
                                && distanceToEdge[i][5] != 0)
                                    checkCounter++;
                                int[] testBoard = MakeMove(i, i + 9, board);
                                if (CheckChecker(testBoard)[kingTile])
                                    continue;
                                if (depth != 0)
                                {
                                    counter += FindAllMoves(testBoard, depth - 1);
                                                            }
                                else
                                    counter++;
                            }
                            if ((board[i + 7] < 6 || board[i - 1] == board[69])
                                && distanceToEdge[i][7] != 0)
                            {
                                if(board[i - 1] == board[69]
                                && distanceToEdge[i][7] != 0)
                                    checkCounter++;
                                int[] testBoard = MakeMove(i, i + 7, board);
                                if (CheckChecker(testBoard)[kingTile])
                                    continue;
                                if (depth != 0)
                                {
                                    counter += FindAllMoves(testBoard, depth - 1);
                                                            }
                                else
                                    counter++;
                            }
                            break;
                        case 8:                                                                                     //knight
                            for (int x = 0; x < 8; x++)
                                if (knightLegalMoves[i, x] && board[i + nMoves[x]] < 7)
                                {
                                    int[] testBoard = MakeMove(i, i + nMoves[x], board);
                                    if (CheckChecker(testBoard)[kingTile])
                                        continue;
                                    if (depth != 0)
                                    {
                                        counter += FindAllMoves(testBoard, depth - 1);
                                    }
                                    else
                                        counter++;
                                    //allPositions[0].Add(new int[] {board[i], i, i + nMoves[x]});
                                    //allPositions[1].Add(testBoard);
                                }
                            break;
                        case 9:                                                                                     //bishop
                            for (int x = 0; x < 4; x++)
                            {
                                int testTile = i;
                                for (int y = 0; y < distanceToEdge[i][x]; y++)
                                {
                                    testTile += bMoves[x];
                                    if (board[testTile] > 6)
                                        break;
                                    int[] testBoard = MakeMove(i, i + bMoves[x], board);
                                    if (CheckChecker(testBoard)[kingTile])
                                        continue;
                                    if (depth != 0)
                                    {
                                        counter += FindAllMoves(testBoard, depth - 1);
                                    }
                                    else
                                        counter++;
                                    //allPositions[0].Add(new int[] {board[i], i, i + bMoves[x]});
                                    //allPositions[1].Add(testBoard);
                                    if (board[testTile] != 6)
                                        break;
                                }
                            }
                            break;
                        case 10:                                                                                     //rook
                            for (int x = 4; x < 8; x++)
                            {
                                int testTile = i;
                                for (int y = 0; y < distanceToEdge[i][x]; y++)
                                {
                                    testTile += rMoves[x - 4];
                                    if (board[testTile] > 6)
                                        break;
                                    int[] testBoard = MakeMove(i, i + rMoves[x - 4], board);
                                    if (CheckChecker(testBoard)[kingTile])
                                        continue;
                                    if (depth != 0)
                                    {
                                        counter += FindAllMoves(testBoard, depth - 1);
                                    }
                                    else
                                        counter++;
                                    //allPositions[0].Add(new int[] {board[i], i, i + rMoves[x - 4]});
                                    //allPositions[1].Add(testBoard);
                                    if (board[testTile] != 6)
                                        break;
                                }
                            }
                            break;
                        case 11:                                                                                     //queen
                            for (int x = 0; x < 8; x++)
                            {
                                int testTile = i;
                                for (int y = 0; y < distanceToEdge[i][x]; y++)
                                {
                                    testTile += qkMoves[x];
                                    if (board[testTile] > 6)
                                        break;
                                    int[] testBoard = MakeMove(i, i + qkMoves[x], board);
                                    if (CheckChecker(testBoard)[kingTile])
                                        continue;
                                    if (depth != 0)
                                    {
                                        counter += FindAllMoves(testBoard, depth - 1);
                                    }
                                    else
                                        counter++;
                                    //allPositions[0].Add(new int[] {board[i], i, i + qkMoves[x]});
                                    //allPositions[1].Add(testBoard);
                                    if (board[testTile] != 6)
                                        break;
                                }
                            }
                            break;
                        case 12:                                                                                     //king
                            foreach (int move in qkMoves)
                            {
                                if (i + move < 0 || i + move > 63 || board[i + move] > 6)
                                    continue;
                                int[] testBoard = MakeMove(i, i + move, board);
                                if (CheckChecker(testBoard)[i + move])
                                    continue;
                                if (depth != 0)
                                {
                                    counter += FindAllMoves(testBoard, depth - 1);
                                                            }
                                else
                                    counter++;
                            }
                            bool[] checkBoard = CheckChecker(board);
                            if (!checkBoard[4])
                            {
                                if (board[68] == 1 && (!checkBoard[3]) && (!checkBoard[2])
                                && board[3] == 6 && board[2] == 6)
                                {
                                    int[] testBoard = MakeMove(4, 2, board);
                                    if (depth != 0)
                                    {
                                        counter += FindAllMoves(testBoard, depth - 1);
                                    }
                                    else
                                        counter++;
                                    //allPositions[0].Add(new int[] {board[i], 4, 2});
                                    //allPositions[1].Add(testBoard);
                                }
                                if (board[67] == 1 && (!checkBoard[5]) && (!checkBoard[6])
                                && board[5] == 6 && board[6] == 6)
                                {
                                    int[] testBoard = MakeMove(4, 6, board);
                                    if (depth != 0)
                                    {
                                        counter += FindAllMoves(testBoard, depth - 1);
                                    }
                                    else
                                        counter++;
                                    //allPositions[0].Add(new int[] {board[i], 4, 6});
                                    //allPositions[1].Add(testBoard);
                                }
                            }
                            break;
                    }
                }
            }
            return counter;
        }
        private bool[] CheckChecker(int[] board)
        {
            bool[] checks = new bool[64];
            for (int i = 0; i < 64; i++)
            {
                if ((board[64] == 1 && board[i] < 7)
                || (board[64] == 0 && board[i] > 5))
                    continue;
                if (board[i] == 0 && i > 7)
                {
                    if (distanceToEdge[i][7] != 0)
                        checks[i - 9] = true;
                    if (distanceToEdge[i][5] != 0)
                        checks[i - 7] = true;
                }
                else if (board[i] == 7 && i < 56)
                {
                    if (distanceToEdge[i][7] != 0)
                        checks[i + 7] = true;
                    if (distanceToEdge[i][5] != 0)
                        checks[i + 9] = true;
                }
                else
                    switch (board[i] % 7)
                    {
                        case 1:
                            for (int x = 0; x < 8; x++)
                                if (knightLegalMoves[i, x])
                                    checks[i + nMoves[x]] = true;
                            break;
                        case 2:
                            for (int x = 0; x < 4; x++)
                            {
                                int testTile = i;
                                for (int y = 0; y < distanceToEdge[i][x]; y++)
                                {
                                    testTile += bMoves[x];
                                    checks[testTile] = true;
                                    if (board[testTile] != 6)
                                        break;
                                }
                            }
                            break;
                        case 3:
                            for (int x = 4; x < 8; x++)
                            {
                                int testTile = i;
                                for (int y = 0; y < distanceToEdge[i][x]; y++)
                                {
                                    testTile += rMoves[x - 4];
                                    checks[testTile] = true;
                                    if (board[testTile] != 6)
                                        break;
                                }
                            }
                            break;
                        case 4:
                            for (int x = 0; x < 8; x++)
                            {
                                int testTile = i;
                                for (int y = 0; y < distanceToEdge[i][x]; y++)
                                {
                                    testTile += qkMoves[x];
                                    checks[testTile] = true;
                                    if (board[testTile] != 6)
                                        break;
                                }
                            }
                            break;
                        case 5:
                            foreach (int d in qkMoves)
                            {
                                if ((!(distanceToEdge[i][7] == 0 && (d == qkMoves[0] || d == qkMoves[3] || d == qkMoves[7])))
                                 && (!(distanceToEdge[i][5] == 0 && (d == qkMoves[1] || d == qkMoves[2] || d == qkMoves[5])))
                                 && (!(distanceToEdge[i][4] == 0 && (d == qkMoves[0] || d == qkMoves[1] || d == qkMoves[4])))
                                 && (!(distanceToEdge[i][6] == 0 && (d == qkMoves[2] || d == qkMoves[3] || d == qkMoves[6]))))
                                    checks[i + d] = true;
                            }
                            break;
                    }
            }
            return checks;
        }
        private double Evaluate(int[] board)
        {
            double eval = 0;
            for (int i = 0; i < 64; i++)
            {
                // valueWeights[board[i] % 7];

            }
            return eval;
        }
        private int[] MakeMove(int sPos, int ePos, int[] oldBoard)
        {
            int[] board = oldBoard.ToList().ToArray();
            board[ePos] = board[sPos];
            if (board[sPos] == 5)
            {
                board[65] = 0;
                board[66] = 0;
                if (ePos == sPos - 2)
                {
                    board[59] = 3;
                    board[56] = 6;

                }
                else if (ePos == sPos + 2)
                {
                    board[61] = 3;
                    board[63] = 6;
                }
            }
            else if (board[sPos] == 12)
            {
                board[67] = 0;
                board[68] = 0;
                if (ePos == sPos - 2)
                {
                    board[3] = 10;
                    board[0] = 6;
                }
                else if (ePos == sPos + 2)
                {
                    board[5] = 10;
                    board[7] = 6;
                }
            }
            else if (board[69] != -999)
                if (ePos + 8 == board[69] && board[sPos] == 0)
                    board[ePos + 8] = 6;
                else if (ePos - 8 == board[69] && board[sPos] == 7)
                    board[ePos - 8] = 6;
            board[64] = 1 - board[64];
            board[65] = board[65] != 0 && board[63] == 3 ? 1 : 0;
            board[66] = board[66] != 0 && board[56] == 3 ? 1 : 0;
            board[67] = board[67] != 0 && board[7] == 10 ? 1 : 0;
            board[68] = board[68] != 0 && board[0] == 10 ? 1 : 0;
            if (sPos - ePos == 16 || ePos - sPos == 16)
                board[69] = ePos;
            else
                board[69] = -999;
            board[sPos] = 6;
            return board;
        }
        private List<int> ConvertList(List<char> charBoard)
        {
            List<int> intBoard = new List<int>();
            foreach (char c in charBoard)
                intBoard.Add(pieceToNum[c]);
            return intBoard;
        }
    }
}