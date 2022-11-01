using System;
using System.Collections.Generic;
using System.Linq;

namespace test
{
    public class AI
    {
        bool whiteOnTurn;
        string[] FENstring;
        readonly int[] bpMoves = new int[] { 7, 8, 9, 16 };
        readonly int[] wpMoves = new int[] {  -7, -8, -9, -16 };
        readonly int[] nMoves = new int[] { -17, -15, -6, 10, 17, 15, 6, -10 };
        readonly int[] bMoves = new int[4] { -9, -7, 9, 7 };
        readonly int[] rMoves = new int[4] { -8, 1, 8, -1 };
        readonly int[] qkMoves = new int[8] { -9, -7, 9, 7, -8, 1, 8, -1 };
        Dictionary<int,char> numToPiece = new Dictionary<int, char>()
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
        Dictionary<char,int> pieceToNum = new Dictionary<char,int>()
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
        public AI(bool whiteOnTurn)
        {
            this.whiteOnTurn = whiteOnTurn;
        }

        public string PlayMove(List<char> position, string[] FENstring)
        {
            this.FENstring = FENstring;
            List<int> board = ConvertList(position);
            board[69] = FENstring[3] != "-" ? Functions.TileToNum(FENstring[3]) : -1;
            return "";
        }

        public int[][] FindAllMoves(int[] board)
        {
            List<int[]> allMoves = new List<int[]>();
            List<int[]> allPositions = new List<int[]>();
                if(board[64] == 0)                                                                                     //white
                {
                    for (int i = 0; i < 64; i++)
                    {   
                        if(board[i] > 5)
                            continue;
                        int[] distanceToEdge = Functions.DistanceToEdge(i);
                        switch(board[i])
                        {
                            case 0:                                                                                    //pawn
                                foreach(int move in wpMoves)
                                {
                                    if (((move == -8 || (move == -16 && i > 47 && board[i - 16] == 6)) 
                                        && board[i - 8] == 6)
                                    || (move == -9 && (board[i - 9] > 6 || board[i - 1] == board[69])
                                        && distanceToEdge[7] != 0)
                                    || (move == -7 && (board[i - 7] > 6 || board[i + 1] == board[69])
                                        && distanceToEdge[5] != 0))
                                    {
                                        int[] testBoard = MakeMove(i, i + move, board);
                                        if(CheckChecker(testBoard)[Array.IndexOf(board, 5)])
                                            continue;
                                        allMoves.Add(new int[] {i, i + move});
                                        allPositions.Add(testBoard);
                                    }
                                }
                                break;
                            case 1:                                                                                     //knight
                                bool[] knightLegalMoves = Functions.LegalKnightMoves(distanceToEdge);
                                for (int x = 0; x < 8; x++)
                                    if (knightLegalMoves[x] && board[i + nMoves[x]] > 5)
                                    {
                                        int[] testBoard = MakeMove(i, i + nMoves[x], board);
                                        if(CheckChecker(testBoard)[Array.IndexOf(board, 5)])
                                            continue;
                                        allMoves.Add(new int[] {i, i + nMoves[x]});
                                        allPositions.Add(testBoard);
                                    }
                                break;
                            case 2:                                                                                     //bishop
                                for (int x = 0; x < 4; x++)
                                {
                                    int testTile = i;
                                    for (int y = 0; y < distanceToEdge[x]; y++)
                                    {
                                        testTile += bMoves[x];
                                        if (board[testTile] < 6)
                                            break;
                                        int[] testBoard = MakeMove(i, i + bMoves[x], board);
                                        if(CheckChecker(testBoard)[Array.IndexOf(board, 5)])
                                            continue;
                                        allMoves.Add(new int[] {i, i + bMoves[x]});
                                        allPositions.Add(testBoard);
                                        if (board[testTile] != 6)
                                            break;
                                    }
                                }
                                break;
                            case 3:                                                                                     //rook
                                for (int x = 4; x < 8; x++)
                                {
                                    int testTile = i;
                                    for (int y = 0; y < distanceToEdge[x]; y++)
                                    {
                                        testTile += rMoves[x - 4];
                                        if (board[testTile] < 6)
                                            break;
                                        int[] testBoard = MakeMove(i, i + rMoves[x - 4], board);
                                        if(CheckChecker(testBoard)[Array.IndexOf(board, 5)])
                                            continue;
                                        allMoves.Add(new int[] {i, i + rMoves[x - 4]});
                                        allPositions.Add(testBoard);
                                        if (board[testTile] != 6)
                                            break;
                                    }
                                }
                                break;
                            case 4:                                                                                     //queen
                                for (int x = 0; x < 8; x++)
                                {
                                    int testTile = i;
                                    for (int y = 0; y < distanceToEdge[x]; y++)
                                    {
                                        testTile += qkMoves[x];
                                        if (board[testTile] < 6)
                                            break;
                                        int[] testBoard = MakeMove(i, i + qkMoves[x], board);
                                        if(CheckChecker(testBoard)[Array.IndexOf(board, 5)])
                                            continue;
                                        allMoves.Add(new int[] {i, i + qkMoves[x]});
                                        allPositions.Add(testBoard);
                                        if (board[testTile] != 6)
                                            break;
                                    }
                                }
                                break;
                            case 5:                                                                                     //king
                                foreach (int move in qkMoves)
                                {
                                    if (board[i + move] < 6)
                                        continue;
                                    int[] testBoard = MakeMove(i, i + move, board);
                                    if(CheckChecker(testBoard)[i + move])
                                        continue;
                                    allMoves.Add(new int[] {i, i + move});
                                    allPositions.Add(testBoard);
                                }
                                bool[] checkBoard = CheckChecker(board);
                                if(!checkBoard[60])
                                {
                                    if(board[65] == 1 && (!checkBoard[59]) && (!checkBoard[58])
                                    && board[59] == 6 && board[58] == 6)
                                    {
                                        allMoves.Add(new int[] {60, 58});
                                        allPositions.Add(MakeMove(60, 58, board));
                                    }
                                    if(board[66] == 1 && (!checkBoard[61]) && (!checkBoard[62])
                                    && board[61] == 6 && board[62] == 6)
                                    {
                                        allMoves.Add(new int[] {60, 62});
                                        allPositions.Add(MakeMove(60, 62, board));
                                    }
                                }
                                break;
                        }
                    }
                }
                else                                                                                                //black
                {   
                    for (int i = 0; i < 64; i++)
                    {   
                        if(board[i] < 7)
                            continue;
                        int[] distanceToEdge = Functions.DistanceToEdge(i);
                        switch(board[i])
                        {
                            case 7:                                                                                     //pawn
                                foreach(int move in wpMoves)
                                {
                                    if (((move == 8 || (move == 16 && i < 16 && board[i + 16] == 6)) 
                                        && board[i + 8] == 6)
                                    || (move == 9 && (board[i + 9] < 6 || board[i + 1] == board[69])
                                        && distanceToEdge[5] != 0)
                                    || (move == 7 && (board[i + 7] < 6 || board[i - 1] == board[69])
                                        && distanceToEdge[7] != 0))
                                    {
                                        int[] testBoard = MakeMove(i, i + move, board);
                                        if(CheckChecker(testBoard)[Array.IndexOf(board, 12)])
                                            continue;
                                        allMoves.Add(new int[] {i, i + move});
                                        allPositions.Add(testBoard);
                                    }
                                }
                                break;
                            case 8:                                                                                     //knight
                                bool[] knightLegalMoves = Functions.LegalKnightMoves(distanceToEdge);
                                for (int x = 0; x < 8; x++)
                                    if (knightLegalMoves[x] && board[i + nMoves[x]] < 7)
                                    {
                                        int[] testBoard = MakeMove(i, i + nMoves[x], board);
                                        if(CheckChecker(testBoard)[Array.IndexOf(board, 12)])
                                            continue;
                                        allMoves.Add(new int[] {i, i + nMoves[x]});
                                        allPositions.Add(testBoard);
                                    }
                                break;
                            case 9:                                                                                     //bishop
                                for (int x = 0; x < 4; x++)
                                {
                                    int testTile = i;
                                    for (int y = 0; y < distanceToEdge[x]; y++)
                                    {
                                        testTile += bMoves[x];
                                        if (board[testTile] > 6)
                                            break;
                                        int[] testBoard = MakeMove(i, i + bMoves[x], board);
                                        if(CheckChecker(testBoard)[Array.IndexOf(board, 12)])
                                            continue;
                                        allMoves.Add(new int[] {i, i + bMoves[x]});
                                        allPositions.Add(testBoard);
                                        if (board[testTile] != 6)
                                            break;
                                    }
                                }
                                break;
                            case 10:                                                                                     //rook
                                for (int x = 4; x < 8; x++)
                                {
                                    int testTile = i;
                                    for (int y = 0; y < distanceToEdge[x]; y++)
                                    {
                                        testTile += rMoves[x - 4];
                                        if (board[testTile] > 6)
                                            break;
                                        int[] testBoard = MakeMove(i, i + rMoves[x - 4], board);
                                        if(CheckChecker(testBoard)[Array.IndexOf(board, 12)])
                                            continue;
                                        allMoves.Add(new int[] {i, i + rMoves[x - 4]});
                                        allPositions.Add(testBoard);
                                        if (board[testTile] != 6)
                                            break;
                                    }
                                }
                                break;
                            case 11:                                                                                     //queen
                                for (int x = 0; x < 8; x++)
                                {
                                    int testTile = i;
                                    for (int y = 0; y < distanceToEdge[x]; y++)
                                    {
                                        testTile += qkMoves[x];
                                        if (board[testTile] > 6)
                                            break;
                                        int[] testBoard = MakeMove(i, i + qkMoves[x], board);
                                        if(CheckChecker(testBoard)[Array.IndexOf(board, 12)])
                                            continue;
                                        allMoves.Add(new int[] {i, i + qkMoves[x]});
                                        allPositions.Add(testBoard);
                                        if (board[testTile] != 6)
                                            break;
                                    }
                                }
                                break;
                            case 12:                                                                                     //king
                                foreach (int move in qkMoves)
                                {
                                    if (board[i + move] > 6)
                                        continue;
                                    int[] testBoard = MakeMove(i, i + move, board);
                                    if(CheckChecker(testBoard)[i + move])
                                        continue;
                                    allMoves.Add(new int[] {i, i + move});
                                    allPositions.Add(testBoard);
                                }
                                bool[] checkBoard = CheckChecker(board);
                                if(!checkBoard[4])
                                {
                                    if(board[67] == 1 && (!checkBoard[3]) && (!checkBoard[2])
                                    && board[3] == 6 && board[2] == 6)
                                    {
                                        allMoves.Add(new int[] {4, 2});
                                        allPositions.Add(MakeMove(4, 2, board));
                                    }
                                    if(board[68] == 1 && (!checkBoard[5]) && (!checkBoard[6])
                                    && board[5] == 6 && board[6] == 6)
                                    {
                                        allMoves.Add(new int[] {4, 6});
                                        allPositions.Add(MakeMove(4, 6, board));
                                    }
                                }
                                break;
                        }
                    }
                }
                return allMoves.ToArray();
        }
        private bool[] CheckChecker(int[] board)
        {
            bool[] checks = new bool[64];
            for (int i = 0; i < 64; i++)
            {
                if ((board[64] == 0 && board[i] < 7 )
                || (board[64] == 1 && board[i] > 5))
                    continue;
                int[] distanceToEdge = Functions.DistanceToEdge(i);
                if (board[i] == 'p' && i > 7)
                {
                    if (distanceToEdge[7] != 0)
                        checks[i - 9] = true;
                    if (distanceToEdge[5] != 0)
                        checks[i - 7] = true;
                }
                else if (board[i] == 'P' && i < 56)
                {
                    if (distanceToEdge[7] != 0)
                        checks[i + 7] = true;
                    if (distanceToEdge[5] != 0)
                        checks[i + 9] = true;
                }
                else
                    switch (board[i] % 7)
                    {
                        case 1:
                            bool[] knightLegalMoves = Functions.LegalKnightMoves(distanceToEdge);
                            for (int x = 0; x < 8; x++)
                                if (knightLegalMoves[x])
                                    checks[i + nMoves[x]] = true;

                            break;
                        case 2:
                            for (int x = 0; x < 4; x++)
                            {
                                int testTile = i;
                                for (int y = 0; y < distanceToEdge[x]; y++)
                                {
                                    testTile += bMoves[x];
                                    checks[testTile] = true;
                                    if (board[testTile] != ' ')
                                        break;
                                }
                            }
                            break;
                        case 3:
                            for (int x = 4; x < 8; x++)
                            {
                                int testTile = i;
                                for (int y = 0; y < distanceToEdge[x]; y++)
                                {
                                    testTile += rMoves[x - 4];
                                    checks[testTile] = true;
                                    if (board[testTile] != ' ')
                                        break;
                                }
                            }
                            break;
                        case 4:
                            for (int x = 0; x < 8; x++)
                            {
                                int testTile = i;
                                for (int y = 0; y < distanceToEdge[x]; y++)
                                {
                                    testTile += qkMoves[x];
                                    checks[testTile] = true;
                                    if (board[testTile] != ' ')
                                        break;
                                }
                            }
                            break;
                        case 5:
                            foreach (int d in qkMoves)
                            {
                                if ((!(distanceToEdge[7] == 0 && (d == qkMoves[0] || d == qkMoves[3] || d == qkMoves[7])))
                                 && (!(distanceToEdge[5] == 0 && (d == qkMoves[1] || d == qkMoves[2] || d == qkMoves[5])))
                                 && (!(distanceToEdge[4] == 0 && (d == qkMoves[0] || d == qkMoves[1] || d == qkMoves[4])))
                                 && (!(distanceToEdge[6] == 0 && (d == qkMoves[2] || d == qkMoves[3] || d == qkMoves[6]))))
                                    checks[i + d] = true;
                            }
                            break;
                    }
            }
            return checks;
        }

        private int[] MakeMove(int sPos, int ePos, int[] board)
        {
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
            else if (board[69] != -1)
                if (ePos + 8 == board[69] &&  board[sPos] == 0)
                    board[ePos + 8] = 6;
                else if (ePos - 8 == board[69] && board[sPos] == 7)
                    board[ePos - 8] = 6;
            board[64] = 1 - board[64];
            board[65] = board[65] != 0 && board[63] == 3 ? 1 : 0;
            board[66] = board[66] != 0 && board[56] == 3 ? 1 : 0;
            board[67] = board[67] != 0 && board[7] == 10 ? 1 : 0;
            board[68] = board[68] != 0 && board[0] == 10 ? 1 : 0;
            if(Math.Abs(sPos - ePos) == 16)
                board[69] = ePos;
            else
                board[69] = -1;
            board[sPos] = 6;
            return board;
        }
        private List<int> ConvertList(List<char> charBoard)
        {
            List<int> intBoard = new List<int>();
            foreach(char c in charBoard)
                intBoard.Add(pieceToNum[c]);
            return intBoard;
        }
    }
}