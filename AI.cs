using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace consoleChess
{
    public readonly struct Move
    {
        public readonly byte startingTile;
        public readonly byte landingTile;
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

        readonly Dictionary<byte, double[]> valueWeights = new(){
            {0, new double[]{0.2, 0.1}},
            {1, new double[]{0.7, 0.3, 0.1}},
            {2, new double[]{0.7, 0.5, 0.1}},
            {3, new double[]{0.5, 0.2}},
            {4, new double[]{0.5, 0.2}},
            {5, new double[]{0.7, 0.3}}
        };
        readonly Dictionary<byte, char> numToPiece = new()
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
        readonly Dictionary<char, byte> pieceToNum = new()
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
        readonly Dictionary<byte, Int16> pieceValues = new(){
            {0, 1}, {7, -1},
            {1, 3}, {8, -3},
            {2, 3}, {9, -3},
            {3, 5}, {10, -5},
            {4, 9}, {11, -9},
            {5, 999}, {12, -999}
        };
        public byte[,] depthmoves = new byte[2, 20];
        public AI(int depth)
        {
            this.depth = depth;
        }
        public List<Move> moves = new();

        public void PlayMove(List<char> position, string[] FENstring)
        {
            List <System.Threading.Thread> threads = new();
            //for(int i = 0; i < System.t)
            Graphics.WriteBoard(position, FENstring);
            List<byte> board = ConvertList(position);
            Board mainBoard = new(
                (byte)(FENstring[1] == "w" ? 0 : 1),
                FENstring[2].Contains('K'),
                FENstring[2].Contains('Q'),
                FENstring[2].Contains('k'),
                FENstring[2].Contains('q'),
                FENstring[3] != "-" ? (byte)Functions.TileToNum(FENstring[3]) : (byte)255,
                (byte)int.Parse(FENstring[4]),
                (byte)int.Parse(FENstring[5]),
                0
            );
            for (byte i = 0; i < 64; i++)
            {
                var currentTile = new Tile(board[i], i, board[i] < 6);
                mainBoard.board[i] = currentTile;
                if (board[i]< 6) mainBoard.whitePieces.Add(currentTile);
                else if (board[i] > 6) mainBoard.blackPieces.Add(currentTile);
            }
            board.Add((byte)(FENstring[1] == "w" ? 0 : 1));
            board.Add((byte)(FENstring[2].Contains('K') ? 1 : 0));
            board.Add((byte)(FENstring[2].Contains('Q') ? 1 : 0));
            board.Add((byte)(FENstring[2].Contains('k') ? 1 : 0));
            board.Add((byte)(FENstring[2].Contains('q') ? 1 : 0));
            board.Add((byte)(FENstring[3] != "-" ? Functions.TileToNum(FENstring[3]) : 255));
            board.Add((byte)int.Parse(FENstring[4]));
            board.Add((byte)int.Parse(FENstring[5]));
            board.Add(0);
            byte[,] checks = GenerateCheckBoard(mainBoard);
            Stopwatch timer = new();
            timer.Start();
            for (byte i = 0; i < 5; i++)
            {
                Console.WriteLine((i + 1) + ". move: " + FindAllMoves(mainBoard.Clone(), checks, i) + $"  Time: {timer.ElapsedMilliseconds} ms.");
            }
            timer.Stop();
        }
        public int FindAllMoves(Board board, byte[,] checkBoard, byte depth)
        {
            int counter = 0;
            byte onTurn = (byte)(1- board.whiteOnTurn);
            if (board.whiteOnTurn == 0)                                                                                     //white
            {
                byte kingTile = (board.whitePieces.FirstOrDefault(x => x.type == 5).position);
                int length = board.whitePieces.Count();
                for(byte i = 0; i < length; i++)
                {
                    switch (board.whitePieces[i].type)
                    {
                        case 0:                                                                                    //pawn
                            sbyte[] wpTiles = {
                                (sbyte)(board.whitePieces[i].position - 8),
                                (sbyte)(board.whitePieces[i].position - 16),
                                (sbyte)(board.whitePieces[i].position - 9),
                                (sbyte)(board.whitePieces[i].position - 7),
                            };
                            if (board.board[wpTiles[0]].type == 6)
                            {
                                NewBranch(ref counter, board.whitePieces[i].position,(byte) wpTiles[0], board, checkBoard, onTurn, kingTile, depth);
                                if (board.whitePieces[i].position > 47 && board.board[wpTiles[1]].type == 6)
                                    NewBranch(ref counter, board.whitePieces[i].position, (byte)wpTiles[1], board, checkBoard, onTurn, kingTile, depth);
                            }
                            if (distanceToEdge[board.whitePieces[i].position][7] != 0 && (board.board[wpTiles[2]].type > 6 || wpTiles[2] == board.enPassantSquare))
                                NewBranch(ref counter, board.whitePieces[i].position, (byte)wpTiles[2], board, checkBoard, onTurn, kingTile, depth);
                            if (distanceToEdge[board.whitePieces[i].position][5] != 0 && (board.board[wpTiles[3]].type > 6 || wpTiles[3] == board.enPassantSquare))
                                NewBranch(ref counter, board.whitePieces[i].position, (byte)wpTiles[3], board, checkBoard, onTurn, kingTile, depth);
                            break;
                        case 1:                                                                                     //knight
                            NewKnightBranch(ref counter, board.whitePieces[i].position, board, checkBoard, onTurn, kingTile, depth);
                            break;
                        case 2:                                                                                     //bishop
                            NewBranchSlidingPieces(ref counter, board.whitePieces[i].position, board, checkBoard, onTurn, kingTile, depth, 0, 4);
                            break;
                        case 3:                                                                                     //rook
                            NewBranchSlidingPieces(ref counter, board.whitePieces[i].position, board, checkBoard, onTurn, kingTile, depth, 4, 8);
                            break;
                        case 4:                                                                                     //queen
                            NewBranchSlidingPieces(ref counter, board.whitePieces[i].position, board, checkBoard, onTurn, kingTile, depth, 0, 8);
                            break;
                        case 5:                                                                                     //king
                            foreach (sbyte kMove in qkMoves)
                            {
                                sbyte testTile = (sbyte)(board.whitePieces[i].position + kMove);
                                if (testTile >= 0 && testTile < 64 && board.board[testTile].type > 5)
                                    NewBranch(ref counter, board.whitePieces[i].position, (byte)testTile, board, checkBoard, onTurn, kingTile, depth);
                            }
                            if (checkBoard[onTurn, 60] != 0)
                            {
                                if (board.whiteCastleQueen && (checkBoard[onTurn, 59] != 0) && (checkBoard[onTurn, 58] != 0)
                                && board.board[59].type == 6 && board.board[58].type == 6 && board.board[57].type == 6)
                                {
                                    Move move = new Move(60, 58);
                                    Board testBoard = MakeMove(move, board);
                                    byte[,] newCheckBoard = CheckUpdate(testBoard, CheckUpdate(testBoard, checkBoard, new Move(56, 59)), move);
                                    if (depth != 0)
                                    {
                                        counter += FindAllMoves(testBoard, newCheckBoard, (byte)(depth - 1));
                                    }
                                    else
                                        counter++;
                                }
                                if (board.whiteCastleKing && (checkBoard[onTurn, 61] != 0) && (checkBoard[onTurn, 62] != 0)
                                && board.board[61].type == 6 && board.board[62].type == 6)
                                {
                                    Move move = new Move(60, 62);
                                    Board testBoard = MakeMove(move, board);
                                    byte[,] newCheckBoard = CheckUpdate(testBoard, CheckUpdate(testBoard, checkBoard, new Move(63, 61)), move);
                                    if (depth != 0)
                                    {
                                        counter += FindAllMoves(testBoard, newCheckBoard, (byte)(depth - 1));
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
                byte kingTile = (board.blackPieces.FirstOrDefault(x => x.type == 12).position);
                int length = board.blackPieces.Count();
                for(int i = 0; i < length; i++)
                {
                    switch (board.blackPieces[i].type)
                    {
                        case 7:                                                                                     //pawn
                            sbyte[] bpTiles = {
                                (sbyte)(board.blackPieces[i].position + 8),
                                (sbyte)(board.blackPieces[i].position + 16),
                                (sbyte)(board.blackPieces[i].position + 9),
                                (sbyte)(board.blackPieces[i].position + 7),
                            };
                            if (board.board[bpTiles[0]].type == 6)
                            {
                                NewBranch(ref counter, board.blackPieces[i].position, (byte)bpTiles[0], board, checkBoard, onTurn, kingTile, depth);
                                if (board.blackPieces[i].position < 16 && board.board[bpTiles[1]].type == 6)
                                    NewBranch(ref counter, board.blackPieces[i].position, (byte)bpTiles[1], board, checkBoard, onTurn, kingTile, depth);
                            }
                            if (distanceToEdge[board.blackPieces[i].position][5] != 0 && (board.board[bpTiles[2]].type < 6 || bpTiles[2] == board.enPassantSquare))
                                NewBranch(ref counter, board.blackPieces[i].position, (byte)bpTiles[2], board, checkBoard, onTurn, kingTile, depth);
                            if (distanceToEdge[board.blackPieces[i].position][7] != 0 && (board.board[bpTiles[3]].type < 6 || bpTiles[3] == board.enPassantSquare))
                                NewBranch(ref counter, board.blackPieces[i].position, (byte)bpTiles[3], board, checkBoard, onTurn, kingTile, depth);
                            break;
                        case 8:                                                                                     //knight
                            NewKnightBranch(ref counter, board.blackPieces[i].position, board, checkBoard, onTurn, kingTile, depth);
                            break;
                        case 9:                                                                                     //bishop
                            NewBranchSlidingPieces(ref counter, board.blackPieces[i].position, board, checkBoard, onTurn, kingTile, depth, 0, 4);
                            break;
                        case 10:                                                                                     //rook
                            NewBranchSlidingPieces(ref counter, board.blackPieces[i].position, board, checkBoard, onTurn, kingTile, depth, 4, 8);
                            break;
                        case 11:                                                                                     //queen
                            NewBranchSlidingPieces(ref counter, board.blackPieces[i].position, board, checkBoard, onTurn, kingTile, depth, 0, 8);
                            break;
                        case 12:                                                                                     //king
                            foreach (sbyte kMove in qkMoves)
                            {
                                sbyte testTile = (sbyte)(board.blackPieces[i].position + kMove);
                                if (testTile < 0 || testTile > 63 || board.board[testTile].type > 6)
                                    continue;
                                Move move = new Move(board.blackPieces[i].position, (byte)testTile);
                                Board testBoard = MakeMove(move, board);
                                byte[,] newCheckBoard = CheckUpdate(testBoard, checkBoard, move);
                                if (newCheckBoard[onTurn, testTile] != 0)
                                    continue;
                                if (depth != 0)
                                {
                                    counter += FindAllMoves(testBoard, newCheckBoard, (byte)(depth - 1));
                                }
                                else
                                    counter++;
                            }
                            if (checkBoard[onTurn, 4] != 0)
                            {
                                if (board.blackCastleQueen && (checkBoard[onTurn, 3] != 0) && (checkBoard[onTurn, 2] != 0)
                                && board.board[3].type == 6 && board.board[2].type == 6 && board.board[1].type == 6)
                                {
                                    Move move = new Move(4, 2);
                                    Board testBoard = MakeMove(move, board);
                                    byte[,] newCheckBoard = CheckUpdate(testBoard, CheckUpdate(testBoard, checkBoard, new Move(0, 3)), move);
                                    if (depth != 0)
                                    {
                                        counter += FindAllMoves(testBoard, newCheckBoard, (byte)(depth - 1));
                                    }
                                    else
                                        counter++;
                                }
                                if (board.blackCastleQueen && (checkBoard[onTurn, 5] != 0) && (checkBoard[onTurn, 6] != 0)
                                && board.board[5].type == 6 && board.board[6].type == 6)
                                {
                                    Move move = new Move(4, 6);
                                    Board testBoard = MakeMove(move, board);
                                    byte[,] newCheckBoard = CheckUpdate(testBoard, CheckUpdate(testBoard, checkBoard, new Move(7, 5)), move);
                                    if (depth != 0)
                                    {
                                        counter += FindAllMoves(testBoard, newCheckBoard, (byte)(depth - 1));
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
        ///    Creates new branch for every legal knight move
        /// </summary>
        public void NewKnightBranch(ref int counter, byte startingTile, Board board, byte[,] checkBoard, byte onTurn, byte kingTile, byte depth)
        {
            for (byte x = 0; x < 8; x++)
            {
                byte testTile = (byte)(startingTile + nMoves[x]);
                if (knightLegalMoves[startingTile, x] && ((board.board[testTile].type > 5 && onTurn == 1 ) || (board.board[testTile].type < 7 && onTurn == 0)))
                    NewBranch(ref counter, startingTile, testTile, board, checkBoard, onTurn, kingTile, depth);
            }
        }
        /// <summary>
        ///    Creates new branch for every legal move for a sliding pieces
        /// </summary>
        private void NewBranchSlidingPieces(ref int counter, byte startingTile, Board board, byte[,] checkBoard, byte onTurn, byte kingTile, byte depth, byte startDirection, byte endDirection)
        {
            for (byte x = startDirection; x < endDirection; x++)
            {
                byte testTile = startingTile;
                for (byte y = 0; y < distanceToEdge[startingTile][x]; y++)
                {
                    testTile = (byte)(testTile + qkMoves[x]);
                    if ((board.board[testTile].type < 6 && onTurn == 1)
                    || (board.board[testTile].type > 6 && onTurn == 0))
                        break;
                    NewBranch(ref counter, startingTile, (byte)(startingTile + qkMoves[x]), board, checkBoard, onTurn, kingTile, depth);
                    if (board.board[testTile].type != 6)
                        break;
                }
            }
        }
        /// <summary>
        ///    Creates new branch for specified move.
        /// </summary>
        public void NewBranch(ref int counter, byte startingTile, byte landingTile, Board board, byte[,] checkBoard, byte onTurn, byte kingTile, byte depth)
        {
            Move move = new Move(startingTile, landingTile);
            Board testBoard = MakeMove(move, board);
            byte[,] newCheckBoard = CheckUpdate(testBoard, checkBoard, move);
            if (newCheckBoard[onTurn, kingTile] == 0)
                if (depth != 0)
                    counter += FindAllMoves(testBoard, newCheckBoard, (byte)(depth - 1));
                else
                    counter++;
        }
        /// <returns>
        /// Updates checkBoard based on the move played.
        /// </returns>
        private byte[,] CheckUpdate(Board board, byte[,] oldCheckBoard, Move move)
        {
            byte[,] checkBoard = oldCheckBoard.Clone() as byte[,];
            byte piece = board.board[move.landingTile].type;
            byte lastOnTurn = (byte)(1-board.whiteOnTurn);
            if (piece == 0)
            {
                checkBoard[lastOnTurn, move.startingTile - 9]--;
                checkBoard[lastOnTurn, move.startingTile - 7]--;
                if (distanceToEdge[move.landingTile][7] != 0)
                    checkBoard[lastOnTurn, move.landingTile - 9]++;
                if (distanceToEdge[move.landingTile][5] != 0)
                    checkBoard[lastOnTurn, move.landingTile - 7]++;
            }
            else if (piece == 7)
            {
                checkBoard[lastOnTurn, move.startingTile + 9]--;
                checkBoard[lastOnTurn, move.startingTile + 7]--;
                if (distanceToEdge[move.landingTile][5] != 0)
                    checkBoard[lastOnTurn, move.landingTile + 9]++;
                if (distanceToEdge[move.landingTile][7] != 0)
                    checkBoard[lastOnTurn, move.landingTile + 7]++;
            }
            else
                switch (piece % 7)
                {
                    case 1:
                        for (byte x = 0; x < 8; x++)
                            if (knightLegalMoves[move.startingTile, x])
                                checkBoard[lastOnTurn, move.startingTile + nMoves[x]]--;
                        for (byte x = 0; x < 8; x++)
                            if (knightLegalMoves[move.landingTile, x])
                                checkBoard[lastOnTurn, move.landingTile + nMoves[x]]++;
                        break;
                    case 2:
                        ARChecks(move.startingTile, ref checkBoard, lastOnTurn, board, 1, 0, 4);
                        ARChecks(move.landingTile, ref checkBoard, lastOnTurn, board, -1, 0, 4);
                        break;
                    case 3:
                        ARChecks(move.startingTile, ref checkBoard, lastOnTurn, board, 1, 4, 8);
                        ARChecks(move.landingTile, ref checkBoard, lastOnTurn, board, -1, 4, 8);
                        break;
                    case 4:
                        ARChecks(move.startingTile, ref checkBoard, lastOnTurn, board, 1, 0, 8);
                        ARChecks(move.landingTile, ref checkBoard, lastOnTurn, board, -1, 0, 8);
                        break;
                    case 5:
                        foreach (sbyte d in qkMoves)
                            if ((!(distanceToEdge[move.startingTile][7] == 0 && (d == qkMoves[0] || d == qkMoves[3] || d == qkMoves[7])))
                             && (!(distanceToEdge[move.startingTile][5] == 0 && (d == qkMoves[1] || d == qkMoves[2] || d == qkMoves[5])))
                             && (!(distanceToEdge[move.startingTile][4] == 0 && (d == qkMoves[0] || d == qkMoves[1] || d == qkMoves[4])))
                             && (!(distanceToEdge[move.startingTile][6] == 0 && (d == qkMoves[2] || d == qkMoves[3] || d == qkMoves[6]))))
                                checkBoard[lastOnTurn, move.startingTile + d]--;
                        foreach (sbyte d in qkMoves)
                            if ((!(distanceToEdge[move.landingTile][7] == 0 && (d == qkMoves[0] || d == qkMoves[3] || d == qkMoves[7])))
                             && (!(distanceToEdge[move.landingTile][5] == 0 && (d == qkMoves[1] || d == qkMoves[2] || d == qkMoves[5])))
                             && (!(distanceToEdge[move.landingTile][4] == 0 && (d == qkMoves[0] || d == qkMoves[1] || d == qkMoves[4])))
                             && (!(distanceToEdge[move.landingTile][6] == 0 && (d == qkMoves[2] || d == qkMoves[3] || d == qkMoves[6]))))
                                checkBoard[lastOnTurn, move.landingTile + d]++;
                        break;
                }
            byte[] positions1 = FindPieces(board, move.startingTile);
            for (byte i = 0; i < 8; i++)
            {
                if (positions1[i] == 255)
                    continue;
                if(board.board[positions1[i]].type == 2 || board.board[positions1[i]].type == 3 || board.board[positions1[i]].type == 4)
                    AddRemove(move.startingTile, Functions.flippedDirections[i], ref checkBoard, 0, 1, board);
                else if (board.board[positions1[i]].type == 9 || board.board[positions1[i]].type == 10 || board.board[positions1[i]].type == 11)
                    AddRemove(move.startingTile, Functions.flippedDirections[i], ref checkBoard, 1, 1, board);
            }
            if (board.movesSinceCapture == 0)
            {
                byte[] positions2 = FindPieces(board, move.landingTile);
                for (byte i = 0; i < 8; i++)
                {
                    if (positions2[i] == 255)
                        continue;
                    if (board.board[positions2[i]].type == 2 || board.board[positions2[i]].type == 3 || board.board[positions2[i]].type == 4)
                        AddRemove(move.landingTile, Functions.flippedDirections[i], ref checkBoard, 0, -1, board);
                    else if (board.board[positions2[i]].type == 9 || board.board[positions2[i]].type == 10 || board.board[positions2[i]].type == 11)
                        AddRemove(move.landingTile, Functions.flippedDirections[i], ref checkBoard, 1, -1, board);
                }
            }
            return checkBoard;
        }
        /// <summary>
        ///    Adds or Removes checks on checkboard for directions specified
        /// </summary>
        private void ARChecks(byte startingTile, ref byte[,] checkBoard, byte lastOnTurn, Board board, sbyte addsub, byte startDirection, byte endDirection)
        {
            for (byte x = startDirection; x < endDirection; x++)
                AddRemove(startingTile, x, ref checkBoard, lastOnTurn, addsub, board);
        }
        /// <summary>
        ///    Adds or Removes checks on checkboard for one direction specified 
        /// </summary>
        private void AddRemove(byte startingTile, byte direction, ref byte[,] checkBoard, byte lastOnTurn, sbyte addsub, Board board)
        {
            byte testTile = startingTile;
            for (byte y = 0; y < distanceToEdge[startingTile][direction]; y++)
            {
                testTile = (byte)(testTile + qkMoves[direction]);
                checkBoard[lastOnTurn, testTile] = (byte)(checkBoard[lastOnTurn, testTile] + addsub);
                if (board.board[testTile].type != 6)
                    break;
            }
        }
        private byte[,] GenerateCheckBoard(Board board)
        {
            byte[,] checks = new byte[2, 64];
            for (byte i = 0; i < 64; i++)
            {
                if (board.board[i].type == 6)
                    continue;
                if (board.board[i].type == 0 && i > 7)
                {
                    if (distanceToEdge[i][7] != 0)
                        checks[board.board[i].type < 6 ? 0 : 1, i - 9]++;
                    if (distanceToEdge[i][5] != 0)
                        checks[board.board[i].type < 6 ? 0 : 1, i - 7]++;
                }
                else if (board.board[i].type == 7 && i < 56)
                {
                    if (distanceToEdge[i][7] != 0)
                        checks[board.board[i].type < 6 ? 0 : 1, i + 7]++;
                    if (distanceToEdge[i][5] != 0)
                        checks[board.board[i].type < 6 ? 0 : 1, i + 9]++;
                }
                else
                    switch (board.board[i].type % 7)
                    {
                        case 1:
                            for (byte x = 0; x < 8; x++)
                                if (knightLegalMoves[i, x])
                                    checks[board.board[i].type < 6 ? 0 : 1, i + nMoves[x]]++;
                            break;
                        case 2:
                            for (byte x = 0; x < 4; x++)
                            {
                                byte testTile = i;
                                for (byte y = 0; y < distanceToEdge[i][x]; y++)
                                {
                                    testTile = (byte)(testTile + bMoves[x]);
                                    checks[board.board[i].type < 6 ? 0 : 1, testTile]++;
                                    if (board.board[testTile].type != 6)
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
                                    checks[board.board[i].type < 6 ? 0 : 1, testTile]++;
                                    if (board.board[testTile].type != 6)
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
                                    checks[board.board[i].type < 6 ? 0 : 1, testTile]++;
                                    if (board.board[testTile].type != 6)
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
                                    checks[board.board[i].type < 6 ? 0 : 1, i + d]++;
                            }
                            break;
                    }
            }
            return checks;
        }
        /// <summary>
        ///     Finds all pieces which could cause discovered attack.  
        /// </summary>
        /// <returns>
        ///     byte index of such pieces.
        /// </returns>
        private byte[] FindPieces(Board board, byte startingTile)
        {
            byte[] tiles = new byte[8];
            for (byte x = 0; x < 8; x++)
            {
                tiles[x] = 255;
                byte testTile = startingTile;
                for (byte y = 0; y < distanceToEdge[startingTile][x]; y++)
                {
                    testTile = (byte)(testTile + qkMoves[x]);
                    if (testTile != 6)
                    {
                        if ((board.board[testTile].type % 7 == 2 && x < 4) || (board.board[testTile].type % 7 == 3 && x > 3) || board.board[testTile].type % 7 == 4)
                            tiles[x] = testTile;
                        break;
                    }
                }
            }
            return tiles;
        }

        /// <summary>
        ///    Evaluates a board based on various conditions. 
        /// </summary>
        private double Evaluate(Board board)
        {
            double eval = 0;
            for (byte i = 0; i < 64; i++)
            {
            }
            return eval;
        }
        /// <returns>
        ///     New board with applied move and updated rules.
        /// </returns>
        private Board MakeMove(Move move, Board oldBoard)
        {
            Board board = oldBoard.Clone();                                                                             // deep copy

            board.halfMoveClock++;                                                                                      // halfmove clock
            board.movesSinceCapture = 0;                                                                                // is capture reset

            if (board.board[move.landingTile].type != 6)                                                                // if capture
            {
                board.halfMoveClock = 0;                                                                                // halfmove clock reset
                board.movesSinceCapture = 1;                                                                            // is capture 
            }
            else if (board.board[move.startingTile].type % 7 == 0)                                                      // if pawn advance
                board.halfMoveClock = 0;                                                                                // halfmove clock reset

            if (board.whiteOnTurn == 1) board.fullMoveClock++;                                                          // fullmove clock

            if (board.movesSinceCapture == 0) (board.whiteOnTurn == 1 ? board.whitePieces : board.blackPieces).RemoveAll(x => x.position == move.landingTile);
            if (board.whiteOnTurn == 0)
            {
                int index = board.whitePieces.FindIndex(x => x.position == move.startingTile);
                board.whitePieces[index] = new Tile(board.whitePieces[index].type, move.landingTile, board.whitePieces[index].isWhite);
            } else {
                int index = board.blackPieces.FindIndex(x => x.position == move.startingTile);
                board.blackPieces[index] = new Tile(board.blackPieces[index].type, move.landingTile, board.blackPieces[index].isWhite);
            }
            board.board[move.landingTile] = board.board[move.startingTile];                                             // set landing tile to new piece
            board.board[move.landingTile].position = move.landingTile;                                                  // update position

            if (board.board[move.startingTile].type == 5)                                                               // if white king moves
            {
                board.whiteCastleKing = false;                                                                          // no castling rights
                board.whiteCastleQueen = false;                                                                        //

                if (move.landingTile == move.startingTile - 2)                                                          // if castle queen side
                {
                    board.board[59].type = 3;                                                                           // change rook
                    board.board[56].type = 6;                                                                          //
                }
                else if (move.landingTile == move.startingTile + 2)                                                     // if castle king side
                {
                    board.board[61].type = 3;                                                                           // change rook
                    board.board[63].type = 6;                                                                          //
                }
            }
            else if (board.board[move.startingTile].type == 12)                                                         // same for black king
            {
                board.blackCastleKing = false;
                board.blackCastleQueen = false;

                if (move.landingTile == move.startingTile - 2)
                {
                    board.board[3].type = 10;
                    board.board[0].type = 6;
                }
                else if (move.landingTile == move.startingTile + 2)
                {
                    board.board[5].type = 10;
                    board.board[7].type = 6;
                }
            }
            else if (board.enPassantSquare != 255 && move.landingTile == board.enPassantSquare)                         // if enPassant
                if (board.board[move.startingTile].type == 0)                                                           // if white Pawn
                    board.board[move.landingTile + 8].type = 6;                                                         // remove pawn under
                else if (board.board[move.startingTile].type == 7)                                                      // if black pawn
                    board.board[move.landingTile - 8].type = 6;                                                         // remove pawn above

            board.whiteOnTurn = (byte)(1 - board.whiteOnTurn);                                                          // onTurn change
            board.whiteCastleKing = board.whiteCastleKing && board.board[63].type == 3;                                 // 
            board.whiteCastleQueen = board.whiteCastleQueen && board.board[56].type == 3;                               //  Update castling rights
            board.blackCastleKing = board.blackCastleKing && board.board[7].type == 10;                                 //
            board.blackCastleQueen = board.blackCastleQueen && board.board[0].type == 10;                               //

            if (move.startingTile - move.landingTile == 16 || move.landingTile - move.startingTile == 16)               // if move is up or down 2 squares
                if (board.board[move.startingTile].type == 0)                                                           // if white pawn
                    board.enPassantSquare = (byte)(move.landingTile + 8);                                               // set enPassant under
                else if (board.board[move.startingTile].type == 7)                                                      // if black pawn
                    board.enPassantSquare = (byte)(move.landingTile - 8);                                               // set enPassant above
                else
                    board.enPassantSquare = 255;                                                                        // 
            else                                                                                                       //  set enPassant to none
                board.enPassantSquare = 255;                                                                          //

            board.board[move.startingTile].type = 6;                                                                    // set starting tile to empty
            return board;
        }
        /// <returns>
        ///    Returns board filled with byte representations of char pieces.
        /// </returns>
        private List<byte> ConvertList(List<char> charBoard)
        {
            List<byte> byteBoard = new List<byte>();
            foreach (char c in charBoard)
                byteBoard.Add(pieceToNum[c]);
            return byteBoard;
        }
    }
}