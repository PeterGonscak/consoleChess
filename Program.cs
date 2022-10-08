using System;
using System.Linq;
using System.Collections.Generic;

namespace test
{
    static class Program
    {
        static readonly int[][] pMoves = new int[2][] {
            new int[4] { 7, 8, 9, 16} ,
            new int[4] { -7, -8, -9, -16}
        };
        static readonly int[][] nMoves = new int[4][] {
            new int[2] { -10, 6 },
            new int[2] { -17, 15 },
            new int[2] { -6, 10 },
            new int[2] { -15, 17 }
        };
        static readonly int[] bMoves = new int[4] { -9, -7, 7, 9 };
        static readonly int[] rMoves = new int[4] { -8, -1, 1, 8 };
        static readonly int[] qkMoves = new int[8] { -9, -7, 7, 9, -8, -1, 1, 8 };
        static readonly string[] bw = new string[] { "w", "b" };
        static bool staleMate = false;
        static double[] score = new double[2] { 0, 0};
        static void Main()
        {
            string[] formatFEN = Functions.StartDialog().Split(" ");
            List<char> board = Functions.GenerateBoard(formatFEN[0]);
            bool[] checks = new bool[64];
            bool gameOn = true;
            while (gameOn)
            {
                Console.Clear();
                Graphics.WriteHint();
                Graphics.WriteBoard(board, Functions.large, formatFEN);
                checks = checkChecker(board, formatFEN[1]);
                if (!CheckMateChecker(board, formatFEN[1], checks))
                {
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
                            int sPos = Functions.TileToNum(move.Substring(0, 2));
                            int ePos = Functions.TileToNum(move.Substring(2, 2));
                            char[] testBoard = board.ToArray();
                            testBoard[ePos] = testBoard[sPos];
                            testBoard[sPos] = ' ';
                            if (((formatFEN[1] == "w" && char.IsLower(board[sPos]) && !checkChecker(testBoard.ToList(), formatFEN[1])[Array.IndexOf(testBoard, 'k')])
                            || (formatFEN[1] == "b" && char.IsUpper(board[sPos]) && !checkChecker(testBoard.ToList(), formatFEN[1])[Array.IndexOf(testBoard, 'K')]))
                            && IsValid(sPos, ePos, board, checks))
                            {
                                if((board[sPos] == 'p' && ePos < 8) || (board[sPos] == 'P' && ePos > 55))
                                    board[ePos] = Functions.SelectPiece(formatFEN[1]);
                                else                
                                    board[ePos] = board[sPos];
                                board[sPos] = ' ';
                                formatFEN[1] = bw[1 - Array.IndexOf(bw, formatFEN[1])];
                                break;
                            }
                        }
                        catch { }
                    }
                }
                else
                {
                    gameOn = false;
                    if(staleMate)
                        Console.WriteLine("StaleMate, its a draw.");
                    else
                        Console.WriteLine("CheckMate, " + (formatFEN[1] == "w" ? "black has won." : "white has won."));
                }
            }
        }
        static bool IsValid(int sPos, int ePos, List<char> board, bool[] checkBoard)
        {
            if (sPos == ePos)
                return false;
            if (board[sPos] == 'p')
                return ((((sPos == ePos + 8 && sPos > 7) || (sPos == ePos + 16 && sPos > 47))
                    && board[ePos] == ' ' && (board[ePos + 8] == ' ' || !(sPos == ePos + 16)))
                || (((sPos == ePos + 7 && (sPos + 1) % 8 != 0)
                    || (sPos == ePos + 9 && sPos % 8 != 0))
                        && char.IsUpper(board[ePos])));
            else if (board[sPos] == 'P')
            {
                return ((((sPos == ePos - 8 && sPos < 56) || (sPos == ePos - 16 && sPos < 16))
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
                            && (!(testTile > 55 && (d == qkMoves[2] || d == qkMoves[3] || d == qkMoves[7]))))
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
                    case 'k':
                        if ((!(sPos % 8 == 0 && (qkMoves[0] == (ePos - sPos)
                                            || qkMoves[2] == (ePos - sPos)
                                            || qkMoves[5] == (ePos - sPos))))
                        && !(sPos % 8 == 7 && (qkMoves[1] == (ePos - sPos)
                                            || qkMoves[3] == (ePos - sPos)
                                            || qkMoves[6] == (ePos - sPos)))
                        && qkMoves.Contains(ePos - sPos) && !checkBoard[ePos]
                        && ((char.IsLower(board[sPos]) && !char.IsLower(board[ePos]))
                        || (char.IsUpper(board[sPos]) && !char.IsUpper(board[ePos]))))
                            return true;
                        return false;
                }
            return false;
        }
        static bool CheckMateChecker(List<char> mainBoard, string onTurn, bool[] checks)
        {
            char[] board = mainBoard.ToArray();
            int sPos = mainBoard.IndexOf(onTurn == "w" ? 'k' : 'K');
            foreach (int d in qkMoves)
            {
                if (sPos + d > -1 && sPos + d < 64 && IsValid(sPos, sPos + d, mainBoard, checks))
                {
                    char[] tryBoard = mainBoard.ToArray();
                    tryBoard[sPos + d] = tryBoard[sPos];
                    tryBoard[sPos] = ' ';
                    if (!checkChecker(tryBoard.ToList(), onTurn)[Array.IndexOf(tryBoard, onTurn == "w" ? 'k' : 'K')])
                        return false;
                }
            }
            for (int i = 0; i < 64; i++)
            {
                char[] testBoard = mainBoard.ToArray();
                if (Functions.IsEnemy(onTurn, board[i]))
                    board[i] = ' ';
                if (board[i] != ' ')
                {
                    if (board[i] == 'p')
                    {
                        foreach (int m in pMoves[1])
                        {
                            if (!((((0 == m + 8 && i > 7) || (0 == m + 16 && i > 47))
                            && mainBoard[i] == ' ' && (mainBoard[i + 8] == ' ' || !(0 == m + 16)))
                            || (((0 == m + 7 && i % 8 != 7)
                            || (0 == m + 9 && i % 8 != 0))
                            && char.IsUpper(mainBoard[i]))))
                                continue;
                            testBoard[i + m] = 'p';
                            testBoard[i] = ' ';
                            if (!checkChecker(testBoard.ToList(), onTurn)[Array.IndexOf(testBoard, 'k')])
                                return false;
                            testBoard = mainBoard.ToArray();
                        }
                    }
                    else if (board[i] == 'P')
                    {
                        foreach (int m in pMoves[0])
                        {
                            if (!((((0 == m - 8 && i < 56)|| (0 == m - 16 && i < 16))
                            && mainBoard[i] == ' ' && (mainBoard[i - 8] == ' ' || !(0 == m - 16)))
                            || (((0 == m - 7 && i % 8 != 0)
                            || (0 == m - 9 && i % 8 != 7))
                            && char.IsLower(mainBoard[i]))))
                                continue;
                            testBoard[i + m] = 'P';
                            testBoard[i] = ' ';
                            if (!checkChecker(testBoard.ToList(), onTurn)[Array.IndexOf(testBoard, 'K')])
                                return false;
                            testBoard = mainBoard.ToArray();
                        }
                        if (i < 16)
                        {
                            testBoard[i + 16] = 'P';
                            testBoard[i] = ' ';
                            if (!checkChecker(testBoard.ToList(), onTurn)[Array.IndexOf(testBoard, 'K')])
                                return false;
                            testBoard = mainBoard.ToArray();
                        }
                    }
                    else
                        switch (char.ToLower(board[i]))
                        {
                            case 'n':
                                if (i % 8 != 0)
                                {
                                    if (i % 8 != 1)
                                    {
                                        if (i > 7)
                                        {
                                            testBoard[i + nMoves[0][0]] = testBoard[i];
                                            testBoard[i] = ' ';
                                            if (!checkChecker(testBoard.ToList(), onTurn)[Array.IndexOf(testBoard, char.IsLower(mainBoard[i]) ? 'k' : 'K')])
                                                return false;
                                            testBoard = mainBoard.ToArray();
                                        }
                                        if (i < 56)
                                        {
                                            testBoard[i + nMoves[0][1]] = testBoard[i];
                                            testBoard[i] = ' ';
                                            if (!checkChecker(testBoard.ToList(), onTurn)[Array.IndexOf(testBoard, char.IsLower(mainBoard[i]) ? 'k' : 'K')])
                                                return false;
                                            testBoard = mainBoard.ToArray();
                                        }
                                    }
                                    if (i > 15)
                                    {
                                        testBoard[i + nMoves[1][0]] = testBoard[i];
                                        testBoard[i] = ' ';
                                        if (!checkChecker(testBoard.ToList(), onTurn)[Array.IndexOf(testBoard, char.IsLower(mainBoard[i]) ? 'k' : 'K')])
                                            return false;
                                        testBoard = mainBoard.ToArray();
                                    }
                                    if (i < 48)
                                    {
                                        testBoard[i + nMoves[1][1]] = testBoard[i];
                                        testBoard[i] = ' ';
                                        if (!checkChecker(testBoard.ToList(), onTurn)[Array.IndexOf(testBoard, char.IsLower(mainBoard[i]) ? 'k' : 'K')])
                                            return false;
                                        testBoard = mainBoard.ToArray();
                                    }
                                }
                                if (i % 8 != 7)
                                {
                                    if (i % 8 != 6)
                                    {
                                        if (i > 7)
                                        {
                                            testBoard[i + nMoves[2][0]] = testBoard[i];
                                            testBoard[i] = ' ';
                                            if (!checkChecker(testBoard.ToList(), onTurn)[Array.IndexOf(testBoard, char.IsLower(mainBoard[i]) ? 'k' : 'K')])
                                                return false;
                                            testBoard = mainBoard.ToArray();
                                        }
                                        if (i < 54)
                                        {
                                            testBoard[i + nMoves[2][1]] = testBoard[i];
                                            testBoard[i] = ' ';
                                            if (!checkChecker(testBoard.ToList(), onTurn)[Array.IndexOf(testBoard, char.IsLower(mainBoard[i]) ? 'k' : 'K')])
                                                return false;
                                            testBoard = mainBoard.ToArray();
                                        }
                                    }
                                    if (i > 15)
                                    {
                                        testBoard[i + nMoves[3][0]] = testBoard[i];
                                        testBoard[i] = ' ';
                                        if (!checkChecker(testBoard.ToList(), onTurn)[Array.IndexOf(testBoard, char.IsLower(mainBoard[i]) ? 'k' : 'K')])
                                            return false;
                                        testBoard = mainBoard.ToArray();
                                    }
                                    if (i < 48)
                                    {
                                        testBoard[i + nMoves[3][1]] = testBoard[i];
                                        testBoard[i] = ' ';
                                        if (!checkChecker(testBoard.ToList(), onTurn)[Array.IndexOf(testBoard, char.IsLower(mainBoard[i]) ? 'k' : 'K')])
                                            return false;
                                        testBoard = mainBoard.ToArray();
                                    }
                                }
                                break;
                            case 'b':
                                foreach (int d in bMoves)
                                {
                                    int testTile = i;
                                    while ((!(testTile % 8 == 0 && (d == bMoves[0] || d == bMoves[2])))
                                    && (!(testTile % 8 == 7 && (d == bMoves[1] || d == bMoves[3])))
                                    && (!(testTile < 8 && (d == bMoves[0] || d == bMoves[1])))
                                    && (!(testTile > 55 && (d == bMoves[2] || d == bMoves[3]))))
                                    {
                                        testTile += d;
                                        if ((char.IsLower(mainBoard[i]) && char.IsLower(mainBoard[testTile]))
                                        || (char.IsUpper(mainBoard[i]) && char.IsUpper(mainBoard[testTile])))
                                            break;
                                        else
                                        {
                                            testBoard[testTile] = testBoard[i];
                                            testBoard[i] = ' ';
                                            if (!checkChecker(testBoard.ToList(), onTurn)[Array.IndexOf(testBoard, char.IsLower(mainBoard[i]) ? 'k' : 'K')])
                                                return false;
                                            testBoard = mainBoard.ToArray();
                                        }
                                        if (mainBoard[testTile] != ' ')
                                            break;
                                    }
                                }
                                break;
                            case 'r':
                                foreach (int d in rMoves)
                                {
                                    int testTile = i;
                                    while ((!(testTile % 8 == 0 && d == rMoves[1]))
                                    && (!(testTile % 8 == 7 && d == rMoves[2]))
                                    && (!(testTile < 8 && d == rMoves[0]))
                                    && (!(testTile > 55 && d == rMoves[3])))
                                    {
                                        testTile += d;
                                        if ((char.IsLower(mainBoard[i]) && char.IsLower(mainBoard[testTile]))
                                        || (char.IsUpper(mainBoard[i]) && char.IsUpper(mainBoard[testTile])))
                                            break;
                                        else
                                        {
                                            testBoard[testTile] = testBoard[i];
                                            testBoard[i] = ' ';
                                            if (!checkChecker(testBoard.ToList(), onTurn)[Array.IndexOf(testBoard, char.IsLower(mainBoard[i]) ? 'k' : 'K')])
                                                return false;
                                            testBoard = mainBoard.ToArray();
                                        }
                                        if (mainBoard[testTile] != ' ')
                                            break;
                                    }
                                }
                                break;
                            case 'q':
                                foreach (int d in qkMoves)
                                {
                                    int testTile = i;
                                    while ((!(testTile % 8 == 0 && (d == qkMoves[0] || d == qkMoves[2] || d == qkMoves[5])))
                                    && (!(testTile % 8 == 7 && (d == qkMoves[1] || d == qkMoves[3] || d == qkMoves[6])))
                                    && (!(testTile < 8 && (d == qkMoves[0] || d == qkMoves[1] || d == qkMoves[4])))
                                    && (!(testTile > 55 && (d == qkMoves[2] || d == qkMoves[3] || d == qkMoves[7]))))
                                    {
                                        testTile += d;
                                        if ((char.IsLower(mainBoard[i]) && char.IsLower(mainBoard[testTile]))
                                        || (char.IsUpper(mainBoard[i]) && char.IsUpper(mainBoard[testTile])))
                                            break;
                                        else
                                        {
                                            testBoard[testTile] = testBoard[i];
                                            testBoard[i] = ' ';
                                            if (!checkChecker(testBoard.ToList(), onTurn)[Array.IndexOf(testBoard, char.IsLower(mainBoard[i]) ? 'k' : 'K')])
                                                return false;
                                            testBoard = mainBoard.ToArray();
                                        }
                                        if (mainBoard[testTile] != ' ')
                                            break;
                                    }
                                }
                                break;
                        }
                }
            }
            if(!checks[sPos])
                staleMate = true;
            return true;
        }
        static bool[] checkChecker(List<char> boardA, string onTurn)
        {
            char[] board = boardA.ToArray();
            bool[] checks = new bool[64];
            for (int i = 0; i < 64; i++)
            {
                if ((onTurn == "w" && char.IsLower(board[i]))
                || (onTurn == "b" && char.IsUpper(board[i])))
                    board[i] = ' ';
                if (board[i] != ' ')
                {
                    if (board[i] == 'p' &&  i > 7)
                    {
                        if (i % 8 != 0)
                            checks[i - 9] = true;
                        if (i % 8 != 7)
                            checks[i - 7] = true;
                    }
                    else if (board[i] == 'P' && i < 56)
                    {
                        if (i % 8 != 0)
                            checks[i + 7] = true;
                        if (i % 8 != 7)
                            checks[i + 9] = true;
                    }
                    else
                        switch (char.ToLower(board[i]))
                        {
                            case 'n':
                                if (i % 8 != 0)
                                {
                                    if (i % 8 != 1)
                                    {
                                        if (i > 7)
                                            checks[i + nMoves[0][0]] = true;
                                        if (i < 56)
                                            checks[i + nMoves[0][1]] = true;
                                    }
                                    if (i > 15)
                                        checks[i + nMoves[1][0]] = true;
                                    if (i < 48)
                                        checks[i + nMoves[1][1]] = true;
                                }
                                if (i % 8 != 7)
                                {
                                    if (i % 8 != 6)
                                    {
                                        if (i > 7)
                                            checks[i + nMoves[2][0]] = true;
                                        if (i < 54)
                                            checks[i + nMoves[2][1]] = true;
                                    }
                                    if (i > 15)
                                        checks[i + nMoves[3][0]] = true;
                                    if (i < 48)
                                        checks[i + nMoves[3][1]] = true;
                                }
                                break;
                            case 'b':
                                foreach (int d in bMoves)
                                {
                                    int testTile = i;
                                    while ((!(testTile % 8 == 0 && (d == bMoves[0] || d == bMoves[2])))
                                    && (!(testTile % 8 == 7 && (d == bMoves[1] || d == bMoves[3])))
                                    && (!(testTile < 8 && (d == bMoves[0] || d == bMoves[1])))
                                    && (!(testTile > 55 && (d == bMoves[2] || d == bMoves[3]))))
                                    {
                                        testTile += d;
                                        checks[testTile] = true;
                                        if (boardA[testTile] != ' ')
                                            break;
                                    }
                                }
                                break;
                            case 'r':
                                foreach (int d in rMoves)
                                {
                                    int testTile = i;
                                    while ((!(testTile % 8 == 0 && d == rMoves[1]))
                                    && (!(testTile % 8 == 7 && d == rMoves[2]))
                                    && (!(testTile < 8 && d == rMoves[0]))
                                    && (!(testTile > 55 && d == rMoves[3])))
                                    {
                                        testTile += d;
                                        checks[testTile] = true;
                                        if (boardA[testTile] != ' ')
                                            break;
                                    }
                                }
                                break;
                            case 'q':
                                foreach (int d in qkMoves)
                                {
                                    int testTile = i;
                                    while ((!(testTile % 8 == 0 && (d == qkMoves[0] || d == qkMoves[2] || d == qkMoves[5])))
                                    && (!(testTile % 8 == 7 && (d == qkMoves[1] || d == qkMoves[3] || d == qkMoves[6])))
                                    && (!(testTile < 8 && (d == qkMoves[0] || d == qkMoves[1] || d == qkMoves[4])))
                                    && (!(testTile > 55 && (d == qkMoves[2] || d == qkMoves[3] || d == qkMoves[7]))))
                                    {
                                        testTile += d;
                                        checks[testTile] = true;
                                        if (boardA[testTile] != ' ')
                                            break;
                                    }
                                }
                                break;
                            case 'k':
                                foreach (int d in qkMoves)
                                {
                                    if ((!(i % 8 == 0 && (d == qkMoves[0] || d == qkMoves[2] || d == qkMoves[5])))
                                    && (!(i % 8 == 7 && (d == qkMoves[1] || d == qkMoves[3] || d == qkMoves[6])))
                                    && (!(i < 8 && (d == qkMoves[0] || d == qkMoves[1] || d == qkMoves[4])))
                                    && (!(i > 55 && (d == qkMoves[2] || d == qkMoves[3] || d == qkMoves[7]))))
                                        checks[i + d] = true;
                                }
                                break;
                        }
                }
            }
            return checks;
        }
    }
}
