using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Runtime.CompilerServices;

namespace test
{
    public struct Move
    {
        public byte startingTile;
        public byte landingTile;
        public Move(byte sTile, byte lTile)
        {
            startingTile = sTile;
            landingTile = lTile;
        }
    }
    public class AI
    {
        static readonly byte[][] distanceToEdge = Functions.DistanceToEdge();
        readonly bool[,] knightLegalMoves = Functions.LegalKnightMoves(distanceToEdge);
        readonly int depth;
        readonly SByte[] nMoves = new SByte[] { -17, -15, -6, 10, 17, 15, 6, -10 };
        readonly SByte[] bMoves = new SByte[4] { -9, -7, 9, 7 };
        readonly SByte[] rMoves = new SByte[4] { -8, 1, 8, -1 };
        readonly SByte[] qkMoves = new SByte[8] { -9, -7, 9, 7, -8, 1, 8, -1 };

        readonly byte[][] pTiles = new byte[][]{
            new byte[]{27, 28, 34, 35, 36, 37},
            new byte[]{33, 26, 19, 20, 29, 38, 42, 43, 44, 45}
        };
        readonly byte[][] nTiles = new byte[][]{
            new byte[]{27, 28, 35, 36},
            new byte[]{19, 20, 26, 29, 34, 37, 43, 44},
            new byte[]{11, 12, 21, 30, 38, 45, 52, 51, 42, 33, 25, 18},
        };
        readonly byte[][] bTiles = new byte[][]{
            new byte[]{41, 42, 49, 50, 45, 46, 53, 54},
            new byte[]{33, 34, 37, 38},
            new byte[]{40, 48, 47, 55}
        };
        readonly byte[][] rTiles = new byte[][] {
            new byte[]{58, 59, 60, 61},
            new byte[]{57, 62}
        };
        readonly byte[][] qTiles = new byte[][]{
            new byte[]{41, 42, 45, 46, 49, 50, 51, 52, 53, 54},
            new byte[]{33, 34, 35, 36, 37, 38, 43, 44},
        };
        readonly byte[][] kTiles = new byte[][]{
            new byte[]{56, 57, 62, 63},
            new byte[]{48, 49, 58, 61, 54, 55}
        };

        readonly Dictionary<byte, double[]> valueWeights = new Dictionary<byte, double[]>(){
            {0, new double[]{0.2, 0.1}},
            {1, new double[]{0.7, 0.3, 0.1}},
            {2, new double[]{0.7, 0.5, 0.1}},
            {3, new double[]{0.5, 0.2}},
            {4, new double[]{0.5, 0.2}},
            {5, new double[]{0.7, 0.3}}
        };
        readonly Dictionary<byte, char> numToPiece = new Dictionary<byte, char>()
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
        readonly Dictionary<char, byte> pieceToNum = new Dictionary<char, byte>()
        {

            {'P', 0},
            {'N', 1},
            {'B', 2},
            {'R', 3},
            {'Q', 4},
            {'K', 5},
            {' ', 6},
            {'p', 7},
            {'n', 8},
            {'b', 9},
            {'r', 10},
            {'q', 11},
            {'k', 12}
        };
        readonly Dictionary<byte, Int16> pieceValues = new Dictionary<byte, Int16>(){
            {0, 1}, {7, -1},
            {1, 3}, {8, -3},
            {2, 3}, {9, -3},
            {3, 5}, {10, -5},
            {4, 9}, {11, -9},
            {5, 999}, {12, -999}
        };
        public AI(int depth)
        {
            this.depth = depth;
        }

        public void PlayMove(List<char> position, string[] FENstring)
        {
            List<byte> board = ConvertList(position);
            board.Add((byte)(FENstring[1] == "w" ? 0 : 1));
            board.Add((byte)(FENstring[2].Contains('k') ? 1 : 0));
            board.Add((byte)(FENstring[2].Contains('q') ? 1 : 0));
            board.Add((byte)(FENstring[2].Contains('K') ? 1 : 0));
            board.Add((byte)(FENstring[2].Contains('Q') ? 1 : 0));
            board.Add((byte)(FENstring[3] != "-" ? Functions.TileToNum(FENstring[3]) : 255));
            board.Add((byte)int.Parse(FENstring[4]));
            board.Add((byte)int.Parse(FENstring[5]));
            Graphics.WriteBoard(position, FENstring);
            Stopwatch timer = new Stopwatch();
            timer.Start();
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine((i + 1) + ". move: " + FindAllMoves(board.ToArray(), i) + $"  Time: {timer.ElapsedMilliseconds} ms.");
            }
            timer.Stop();
        }
        public int FindAllMoves(byte[] board, int depth)
        {
            int counter = 0;
            if (board[64] == 0)                                                                                     //white
            {
                byte kingTile = (byte)(Array.IndexOf(board, (byte)5));
                for (byte i = 0; i < 64; i++)
                {
                    if (board[i] > 5)
                        continue;
                    switch (board[i])
                    {
                        case 0:                                                                                    //pawn
                            sbyte[] wpTiles = {
                                (sbyte)(i - 8),
                                (sbyte)(i - 16),
                                (sbyte)(i - 9),
                                (sbyte)(i - 7),
                            };
                            if (board[wpTiles[0]] == 6)
                            {
                                Move move = new Move(i, (byte)(wpTiles[0]));
                                byte[] testBoard = MakeMove(move, board);
                                if (CheckChecker(testBoard)[kingTile])
                                    continue;
                                if (depth != 0)
                                {
                                    counter += FindAllMoves(testBoard, depth - 1);
                                }
                                else
                                    counter++;
                                if (i > 47 && board[wpTiles[1]] == 6)
                                {
                                    move.landingTile -= 8;
                                    testBoard = MakeMove(move, board);
                                    if (CheckChecker(testBoard)[kingTile])
                                        continue;
                                    if (depth != 0)
                                    {
                                        counter += FindAllMoves(testBoard, depth - 1);
                                    }
                                    else
                                        counter++;
                                }
                            }
                            if ((board[wpTiles[2]] > 6 || wpTiles[2] == board[69])
                                && distanceToEdge[i][7] != 0)
                            {
                                Move move = new Move(i, (byte)(wpTiles[2]));
                                byte[] testBoard = MakeMove(move, board);
                                if (CheckChecker(testBoard)[kingTile])
                                    continue;
                                if (depth != 0)
                                {
                                    counter += FindAllMoves(testBoard, depth - 1);
                                }
                                else
                                    counter++;
                            }
                            if ((board[wpTiles[3]] > 6 || wpTiles[3] == board[69])
                                && distanceToEdge[i][5] != 0)
                            {
                                Move move = new Move(i, (byte)(wpTiles[3]));
                                byte[] testBoard = MakeMove(move, board);
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
                            for (byte x = 0; x < 8; x++)
                                if (knightLegalMoves[i, x] && board[i + nMoves[x]] > 5)
                                {
                                    Move move = new Move(i, (byte)(i + nMoves[x]));
                                    byte[] testBoard = MakeMove(move, board);
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
                        case 2:                                                                                     //bishop
                            for (byte x = 0; x < 4; x++)
                            {
                                byte testTile = i;
                                for (byte y = 0; y < distanceToEdge[i][x]; y++)
                                {
                                    testTile = (byte)(testTile + bMoves[x]);
                                    if (board[testTile] < 6)
                                        break;
                                    Move move = new Move(i, (byte)(i + bMoves[x]));
                                    byte[] testBoard = MakeMove(move, board);
                                    if (CheckChecker(testBoard)[kingTile])
                                        continue;
                                    if (depth != 0)
                                    {
                                        counter += FindAllMoves(testBoard, depth - 1);
                                    }
                                    else
                                        counter++;
                                    if (board[testTile] != 6)
                                        break;
                                }
                            }
                            break;
                        case 3:                                                                                     //rook
                            for (byte x = 4; x < 8; x++)
                            {
                                byte testTile = i;
                                for (byte y = 0; y < distanceToEdge[i][x]; y++)
                                {
                                    testTile = (byte)(testTile + rMoves[x - 4]);
                                    if (board[testTile] < 6)
                                        break;
                                    Move move = new Move(i, (byte)(i + rMoves[x - 4]));
                                    byte[] testBoard = MakeMove(move, board);
                                    if (CheckChecker(testBoard)[kingTile])
                                        continue;
                                    if (depth != 0)
                                    {
                                        counter += FindAllMoves(testBoard, depth - 1);
                                    }
                                    else
                                        counter++;
                                    if (board[testTile] != 6)
                                        break;
                                }
                            }
                            break;
                        case 4:                                                                                     //queen
                            for (byte x = 0; x < 8; x++)
                            {
                                byte testTile = i;
                                for (byte y = 0; y < distanceToEdge[i][x]; y++)
                                {
                                    testTile = (byte)(testTile + qkMoves[x]);
                                    if (board[testTile] < 6)
                                        break;
                                    Move move = new Move(i, (byte)(i + qkMoves[x]));
                                    byte[] testBoard = MakeMove(move, board);
                                    if (CheckChecker(testBoard)[kingTile])
                                        continue;
                                    if (depth != 0)
                                    {
                                        counter += FindAllMoves(testBoard, depth - 1);
                                    }
                                    else
                                        counter++;
                                    if (board[testTile] != 6)
                                        break;
                                }
                            }
                            break;
                        case 5:                                                                                     //king
                            foreach (sbyte kMove in qkMoves)
                            {
                                sbyte testTile = (sbyte)(i + kMove);
                                if (testTile < 0 || testTile > 63 || board[testTile] < 6)
                                    continue;
                                Move move = new Move(i, (byte)testTile);
                                byte[] testBoard = MakeMove(move, board);
                                if (CheckChecker(testBoard)[testTile])
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
                                && board[59] == 6 && board[58] == 6 && board[57] == 6)
                                {
                                    Move move = new Move(60, 58);
                                    byte[] testBoard = MakeMove(move, board);
                                    if (depth != 0)
                                    {
                                        counter += FindAllMoves(testBoard, depth - 1);
                                    }
                                    else
                                        counter++;
                                }
                                if (board[65] == 1 && (!checkBoard[61]) && (!checkBoard[62])
                                && board[61] == 6 && board[62] == 6)
                                {
                                    Move move = new Move(60, 62);
                                    byte[] testBoard = MakeMove(move, board);
                                    if (depth != 0)
                                    {
                                        counter += FindAllMoves(testBoard, depth - 1);
                                    }
                                    else
                                        counter++;
                                }
                            }
                            break;
                    }
                }
            }
            else                                                                                                //black
            {
                byte kingTile = (byte)Array.IndexOf(board, (byte)12);
                for (byte i = 0; i < 64; i++)
                {
                    if (board[i] < 7)
                        continue;
                    switch (board[i])
                    {
                        case 7:                                                                                     //pawn
                            sbyte[] bpTiles = {
                                (sbyte)(i + 8),
                                (sbyte)(i + 16),
                                (sbyte)(i + 9),
                                (sbyte)(i + 7),
                            };
                            if (i < 16 && board[bpTiles[1]] == 6 && board[bpTiles[0]] == 6)
                            {
                                Move move = new Move(i, (byte)bpTiles[1]);
                                byte[] testBoard = MakeMove(move, board);
                                if (CheckChecker(testBoard)[kingTile])
                                    continue;
                                if (depth != 0)
                                {
                                    counter += FindAllMoves(testBoard, depth - 1);
                                }
                                else
                                    counter++;
                            }
                            if (board[bpTiles[0]] == 6)
                            {
                                Move move = new Move(i, (byte)bpTiles[0]);
                                byte[] testBoard = MakeMove(move, board);
                                if (CheckChecker(testBoard)[kingTile])
                                    continue;
                                if (depth != 0)
                                {
                                    counter += FindAllMoves(testBoard, depth - 1);
                                }
                                else
                                    counter++;
                            }
                            if (distanceToEdge[i][5] != 0 
                            && (board[bpTiles[2]] < 6 || bpTiles[2] == board[69]))
                            {
                                Move move = new Move(i, (byte)bpTiles[2]);
                                byte[] testBoard = MakeMove(move, board);
                                if (CheckChecker(testBoard)[kingTile])
                                    continue;
                                if (depth != 0)
                                {
                                    counter += FindAllMoves(testBoard, depth - 1);
                                }
                                else
                                    counter++;
                            }
                            if (distanceToEdge[i][7] != 0 
                            && (board[bpTiles[3]] < 6 || bpTiles[3] == board[69]))
                            {
                                Move move = new Move(i, (byte)bpTiles[3]);
                                byte[] testBoard = MakeMove(move, board);
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
                            for (byte x = 0; x < 8; x++)
                            {
                                byte testTile = (byte)(i + nMoves[x]);
                                if (knightLegalMoves[i, x] && board[testTile] < 7)
                                {
                                    Move move = new Move(i, testTile);
                                    byte[] testBoard = MakeMove(move, board);
                                    if (CheckChecker(testBoard)[kingTile])
                                        continue;
                                    if (depth != 0)
                                    {
                                        counter += FindAllMoves(testBoard, depth - 1);
                                    }
                                    else
                                        counter++;
                                }
                            }
                            break;
                        case 9:                                                                                     //bishop
                            for (byte x = 0; x < 4; x++)
                            {
                                byte testTile = i;
                                for (byte y = 0; y < distanceToEdge[i][x]; y++)
                                {
                                    testTile = (byte)(testTile + bMoves[x]);
                                    if (board[testTile] > 6)
                                        break;
                                    Move move = new Move(i, testTile);
                                    byte[] testBoard = MakeMove(move, board);
                                    if (CheckChecker(testBoard)[kingTile])
                                        continue;
                                    if (depth != 0)
                                    {
                                        counter += FindAllMoves(testBoard, depth - 1);
                                    }
                                    else
                                        counter++;
                                    if (board[testTile] != 6)
                                        break;
                                }
                            }
                            break;
                        case 10:                                                                                     //rook
                            for (byte x = 4; x < 8; x++)
                            {
                                byte testTile = i;
                                for (byte y = 0; y < distanceToEdge[i][x]; y++)
                                {
                                    testTile = (byte)(testTile + rMoves[x - 4]);
                                    if (board[testTile] > 6)
                                        break;
                                    Move move = new Move(i, testTile);
                                    byte[] testBoard = MakeMove(move, board);
                                    if (CheckChecker(testBoard)[kingTile])
                                        continue;
                                    if (depth != 0)
                                    {
                                        counter += FindAllMoves(testBoard, depth - 1);
                                    }
                                    else
                                        counter++;
                                    if (board[testTile] != 6)
                                        break;
                                }
                            }
                            break;
                        case 11:                                                                                     //queen
                            for (byte x = 0; x < 8; x++)
                            {
                                byte testTile = i;
                                for (byte y = 0; y < distanceToEdge[i][x]; y++)
                                {
                                    testTile = (byte)(testTile + qkMoves[x]);
                                    if (board[testTile] > 6)
                                        break;
                                    Move move = new Move(i, testTile);
                                    byte[] testBoard = MakeMove(move, board);
                                    if (CheckChecker(testBoard)[kingTile])
                                        continue;
                                    if (depth != 0)
                                    {
                                        counter += FindAllMoves(testBoard, depth - 1);
                                    }
                                    else
                                        counter++;
                                    if (board[testTile] != 6)
                                        break;
                                }
                            }
                            break;
                        case 12:                                                                                     //king
                            foreach (sbyte kMove in qkMoves)
                            {
                                sbyte testTile = (sbyte)(i + kMove);
                                if (testTile < 0 || testTile > 63 || board[testTile] > 6)
                                    continue;
                                Move move = new Move(i, (byte)testTile);
                                byte[] testBoard = MakeMove(move, board);
                                if (CheckChecker(testBoard)[testTile])
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
                                && board[3] == 6 && board[2] == 6 && board[1] == 6)
                                {
                                    Move move = new Move(4, 2);
                                    byte[] testBoard = MakeMove(move, board);
                                    if (depth != 0)
                                    {
                                        counter += FindAllMoves(testBoard, depth - 1);
                                    }
                                    else
                                        counter++;
                                }
                                if (board[67] == 1 && (!checkBoard[5]) && (!checkBoard[6])
                                && board[5] == 6 && board[6] == 6)
                                {
                                    Move move = new Move(4, 6);
                                    byte[] testBoard = MakeMove(move, board);
                                    if (depth != 0)
                                    {
                                        counter += FindAllMoves(testBoard, depth - 1);
                                    }
                                    else
                                        counter++;
                                }
                            }
                            break;
                    }
                }
            }
            return counter;
        }
        /// <summary>
        /// Updates checkBoard based on the moved played.
        /// </summary>
        private bool[] CheckUpdate(byte[] board, byte[] checkBoard, Move move)
        {
            byte piece = board[move.startingTile];
            if (piece == 0)
            {
                checkBoard[move.startingTile - 9]--;
                checkBoard[move.startingTile - 7]--;
                if (distanceToEdge[move.landingTile][7] != 0)
                    checkBoard[move.landingTile - 9]++; 
                if (distanceToEdge[move.landingTile][5] != 0)
                    checkBoard[move.landingTile - 7]++;
                foreach(byte slidingPieceIndex in FindSlidingPieces(board, move))
                {

                }
            }
                
                bool[] checks = new bool[64];
            for (byte i = 0; i < 64; i++)
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
                            for (byte x = 0; x < 8; x++)
                                if (knightLegalMoves[i, x])
                                    checks[i + nMoves[x]] = true;
                            break;
                        case 2:
                            for (byte x = 0; x < 4; x++)
                            {
                                byte testTile = i;
                                for (byte y = 0; y < distanceToEdge[i][x]; y++)
                                {
                                    testTile = (byte)(testTile + bMoves[x]);
                                    checks[testTile] = true;
                                    if (board[testTile] != 6)
                                        break;
                                }
                            }
                            break;
                        case 3:
                            for (byte x = 4; x < 8; x++)
                            {
                                byte testTile = i;
                                for (byte y = 0; y < distanceToEdge[i][x]; y++)
                                {
                                    testTile = (byte)(testTile + rMoves[x - 4]);
                                    checks[testTile] = true;
                                    if (board[testTile] != 6)
                                        break;
                                }
                            }
                            break;
                        case 4:
                            for (byte x = 0; x < 8; x++)
                            {
                                byte testTile = i;
                                for (byte y = 0; y < distanceToEdge[i][x]; y++)
                                {
                                    testTile = (byte)(testTile + qkMoves[x]);
                                    checks[testTile] = true;
                                    if (board[testTile] != 6)
                                        break;
                                }
                            }
                            break;
                        case 5:
                            foreach (sbyte d in qkMoves)
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
        private byte[] AddChecks(byte[] checkBoard, byte index, byte direction)
        {

        }
        private byte[] FindSlidingPieces(byte[] board, Move move)
        {

        }
        private double Evaluate(byte[] board)
        {
            double eval = 0;
            for (byte i = 0; i < 64; i++)
            {
            }
            return eval;
        }
        /// <summary>
        /// returns new board with applied move.
        /// </summary>
        private byte[] MakeMove(Move move, byte[] oldBoard)
        {
            byte[] board = oldBoard.ToList().ToArray();                                                                 // deep copy

            board[70] = (byte) (board[move.landingTile] != 6 || board[move.landingTile] % 7 == 0 ? 0 : board[70] + 1);  // halfmove clock

            if (board[64] == 1) board[71]++;                                                                            // fullmove clock

            board[move.landingTile] = board[move.startingTile];                                                         // set landing tile to new piece
            
            if (board[move.startingTile] == 5)                                                                          // if white king moves
            {
                board[65] = 0;                                                                                          // no castling rights
                board[66] = 0;                                                                                         //

                if (move.landingTile == move.startingTile - 2)                                                          // if castle queen side
                {
                    board[59] = 3;                                                                                      // change rook
                    board[56] = 6;                                                                                     //
                }
                else if (move.landingTile == move.startingTile + 2)                                                     // if castle king side
                {
                    board[61] = 3;                                                                                      // change rook
                    board[63] = 6;                                                                                     //
                }
            }
            else if (board[move.startingTile] == 12)                                                                    // same for black king
            {
                board[67] = 0;
                board[68] = 0;

                if (move.landingTile == move.startingTile - 2)
                {
                    board[3] = 10;
                    board[0] = 6;
                }
                else if (move.landingTile == move.startingTile + 2)
                {
                    board[5] = 10;
                    board[7] = 6;
                }
            }
            else if (board[69] != 255 && move.landingTile == board[69])                                                 // if enPassant
                if (board[move.startingTile] == 0)                                                                      // if white Pawn
                    board[move.landingTile + 8] = 6;                                                                    // remove pawn under
                else if (board[move.startingTile] == 7)                                                                 // if black pawn
                    board[move.landingTile - 8] = 6;                                                                    // remove pawn above

            board[64] = (byte)(1 - board[64]);                                                                          // onTurn change
            board[65] = (byte)(board[65] != 0 && board[63] == 3 ? 1 : 0);                                               // 
            board[66] = (byte)(board[66] != 0 && board[56] == 3 ? 1 : 0);                                              //  Update castling rights
            board[67] = (byte)(board[67] != 0 && board[7] == 10 ? 1 : 0);                                             //
            board[68] = (byte)(board[68] != 0 && board[0] == 10 ? 1 : 0);                                            //

            if (move.startingTile - move.landingTile == 16 || move.landingTile - move.startingTile == 16)               // if move is up or down 2 squares
                if (board[move.startingTile] == 0)                                                                      // if white pawn
                    board[69] = (byte)(move.landingTile + 8);                                                           // set enPassant under
                else if (board[move.startingTile] == 7)                                                                 // if black pawn
                    board[69] = (byte)(move.landingTile - 8);                                                           // set enPassant above
                else
                    board[69] = 255;                                                                                    // 
            else                                                                                                       //  set enPassant to none
                board[69] = 255;                                                                                      //

            board[move.startingTile] = 6;                                                                               // set starting tile to empty

            return board;
        }
        private List<byte> ConvertList(List<char> charBoard)
        {
            List<byte> byteBoard = new List<byte>();
            foreach (char c in charBoard)
                byteBoard.Add(pieceToNum[c]);
            return byteBoard;
        }
    }
}