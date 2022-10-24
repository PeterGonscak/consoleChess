namespace test
{
    public static class Program
    {
        static readonly int[][] pMoves = new int[2][] {
            new int[4] { 7, 8, 9, 16},
            new int[4] { -7, -8, -9, -16}
        };
        static readonly int[] nMoves = new int[] {-17, -15, -6, 10, 17, 15,  6, -10 };
        static readonly int[] bMoves = new int[4] {-9, -7, 9, 7 };
        static readonly int[] rMoves = new int[4] { -8, 1, 8, -1 };
        static readonly int[] qkMoves = new int[8] { -9, -7, 9, 7, -8, 1, 8, -1 };
        static readonly string[] bw = new string[] { "w", "b" };
        static bool staleMate = false;
        static double[] score = new double[2] { 0, 0};
        static void Main()
        {
            string[] formatFEN = Graphics.StartDialog();
            List<char> board = Functions.GenerateBoard(formatFEN[0]);
            bool[] checks = new bool[64];
            bool gameOn = true;
            string ans = Graphics.PlayAI();
            if(ans == "no")
                while (gameOn)
                {
                    Console.Clear();
                    Graphics.WriteHint();
                    Graphics.WriteBoard(board, formatFEN);
                    checks = checkChecker(board, formatFEN[1]);
                    if (!CheckMateChecker(board, formatFEN, checks))
                    {
                        Console.WriteLine(formatFEN[1] + " to move.");
                        while (true)
                        {
                            Console.Write("Enter a valid move: ");
                            string move = "" + Console.ReadLine();
                            if (move == "resign")
                            {
                                gameOn = false;
                                break;
                            }
                            try{
                                int sPos = Functions.TileToNum(move.Substring(0, 2));
                                int ePos = Functions.TileToNum(move.Substring(2, 2));
                                char[] testBoard = ChangePiece(board.ToArray(), sPos, ePos, formatFEN[3]);
                                if (((formatFEN[1] == "w" && char.IsLower(board[sPos]))
                                || (formatFEN[1] == "b" && char.IsUpper(board[sPos])))
                                && IsValid(sPos, ePos, board, checks, formatFEN))
                                {
                                    if(char.ToLower(board[sPos]) == 'p' || board[ePos] != ' ')
                                        formatFEN[4] = "0";
                                    else
                                        formatFEN[4] = (int.Parse(formatFEN[4]) + 1 ).ToString();
                                    if((board[sPos] == 'p' && ePos < 8) || (board[sPos] == 'P' && ePos > 55))
                                        board[sPos] = Functions.SelectPiece(formatFEN[1]);
                                    board = ChangePiece(board.ToArray(), sPos, ePos, formatFEN[3]).ToList();
                                    if(formatFEN[1] == "b")
                                        formatFEN[5] = (int.Parse(formatFEN[5]) + 1).ToString();
                                    formatFEN[2] = CastlingRights(formatFEN[2], sPos, ePos, board);
                                    formatFEN[3] = ((sPos == ePos + 16 && sPos > 47) || (sPos == ePos - 16 && sPos < 16) ? Functions.NumToTile(ePos) : "-");
                                    formatFEN[1] = bw[1 - Array.IndexOf(bw, formatFEN[1])];
                                    break;
                                }
                            }
                            catch{}
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
            else
            {
                Random r = new Random();
                if(ans == "r")
                    ans = "wb"[r.Next(2)].ToString();
                AI ai = new AI(bw[1 - Array.IndexOf(bw, ans)]);
                while (gameOn)
                {
                    Console.Clear();
                    Graphics.WriteHint();
                    Graphics.WriteBoard(board, formatFEN);
                    checks = checkChecker(board, formatFEN[1]);
                    if (!CheckMateChecker(board, formatFEN, checks))
                    {
                        Console.WriteLine(formatFEN[1] + " to move.");
                        while (true)
                        {
                            string move;
                            if(formatFEN[1] == ans)
                            {
                                Console.Write("Enter a valid move: ");
                                move = "" + Console.ReadLine();
                            }
                            else
                                move = ai.PlayMove();
                            if (move == "resign")
                            {
                                gameOn = false;
                                break;
                            }
                                int sPos = Functions.TileToNum(move.Substring(0, 2));
                                int ePos = Functions.TileToNum(move.Substring(2, 2));
                                char[] testBoard = ChangePiece(board.ToArray(), sPos, ePos, formatFEN[3]);
                                if (((formatFEN[1] == "w" && char.IsLower(board[sPos]))
                                || (formatFEN[1] == "b" && char.IsUpper(board[sPos])))
                                && IsValid(sPos, ePos, board, checks, formatFEN))
                                {
                                    if(char.ToLower(board[sPos]) == 'p' || board[ePos] != ' ')
                                        formatFEN[4] = "0";
                                    else
                                        formatFEN[4] = (int.Parse(formatFEN[4]) + 1 ).ToString();
                                    if((board[sPos] == 'p' && ePos < 8) || (board[sPos] == 'P' && ePos > 55))
                                        board[sPos] = Functions.SelectPiece(formatFEN[1]);
                                    board = ChangePiece(board.ToArray(), sPos, ePos, formatFEN[3]).ToList();
                                    if(formatFEN[1] == "b")
                                        formatFEN[5] = (int.Parse(formatFEN[5]) + 1).ToString();
                                    formatFEN[2] = CastlingRights(formatFEN[2], sPos, ePos, board);
                                    formatFEN[3] = ((sPos == ePos + 16 && sPos > 47) || (sPos == ePos - 16 && sPos < 16) ? Functions.NumToTile(ePos) : "-");
                                    formatFEN[1] = bw[1 - Array.IndexOf(bw, formatFEN[1])];
                                    break;
                                }
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
        }
        static string CastlingRights(string s, int sPos, int ePos, List<char> board)
        {
            if(s == "-")
                return s;
            switch(board[ePos])
            {
                case 'k':
                    if(sPos == 5)
                        s = s.Remove(s.IndexOf('q'),1).Remove(s.IndexOf('k'),1);
                    if(s == "")
                        return "-";
                    return s;
                case 'K':
                    if(sPos == 60)
                        s = s.Remove(s.IndexOf('Q'),1).Remove(s.IndexOf('K'),1);
                    if(s == "")
                        return "-";
                    return s;
                case 'r':
                    if(sPos == 63)
                        s = s.Remove(s.IndexOf('k'));
                    else if (sPos == 56)
                        s = s.Remove(s.IndexOf('q'));
                    if(s == "")
                        return "-";
                    return s;
                case 'R':
                    if(sPos == 63)
                        s = s.Remove(s.IndexOf('K'));

                    else if (sPos == 56)
                        s = s.Remove(s.IndexOf('Q'));
                    if(s == "")
                        return "-";
                    return s;
                default:
                    if(s == "")
                        return "-";
                    return s;
            }
        }
        static char[] ChangePiece(char[] board, int sPos, int ePos, string enP)
        {
            board[ePos] = board[sPos];
            if(enP != "-")
                if(board[sPos] == 'p' && ePos + 8 == Functions.TileToNum(enP))
                    board[ePos + 8] = ' ';
                else if(board[sPos] == 'P' && ePos - 8 == Functions.TileToNum(enP))
                    board[ePos - 8] = ' ';
            if(board[sPos] == 'k')
            {
                if(ePos == sPos - 2)
                {
                    board[59] = 'r';
                    board[56] = ' ';
                }  
                else if(ePos == sPos + 2)
                {
                    board[61] = 'r';
                    board[63] = ' ';
                }
            }
            else if(board[sPos] == 'K')
            {
                if(ePos == sPos - 2)
                {
                    board[3] = 'R';
                    board[0] = ' ';
                }  
                else if(ePos == sPos + 2)
                {
                    board[5] = 'R';
                    board[7] = ' ';
                }
            }
            board[sPos] = ' ';
            return board;
        }
        static bool IsValid(int sPos, int ePos, List<char> board, bool[] checkBoard, string[] formatFEN)
        {
            if (sPos == ePos)
                return false;
            char[] testBoard = ChangePiece(board.ToArray(), sPos, ePos,formatFEN[3]);
            if((formatFEN[1] == "w" && checkChecker(testBoard.ToList(), formatFEN[1])[Array.IndexOf(testBoard, 'k')])
            || (formatFEN[1] == "b") && checkChecker(testBoard.ToList(), formatFEN[1])[Array.IndexOf(testBoard, 'K')])
                return false;
            int[] distanceToEdge = Functions.DistanceToEdge(sPos);
            if (board[sPos] == 'p')
                return ((((sPos == ePos + 8 && sPos > 7) || (sPos == ePos + 16 && sPos > 47))
                    && board[ePos] == ' ' && (board[ePos + 8] == ' ' || !(sPos == ePos + 16)))
                || (((sPos == ePos + 7 && distanceToEdge[5] != 0)
                    || (sPos == ePos + 9 && distanceToEdge[7] != 0))
                        && (char.IsUpper(board[ePos]) || (formatFEN[3] != "-" && ePos + 8 == Functions.TileToNum(formatFEN[3])))));
            else if (board[sPos] == 'P')
                return ((((sPos == ePos - 8 && sPos < 56) || (sPos == ePos - 16 && sPos < 16))
                    && board[ePos] == ' ' && (board[ePos - 8] == ' ' || !(sPos == ePos - 16)))
                || (((sPos == ePos - 7 && distanceToEdge[7] != 0)
                    || (sPos == ePos - 9 && distanceToEdge[5] != 0))
                        && (char.IsLower(board[ePos]) || (formatFEN[3] != "-" && ePos - 8 == Functions.TileToNum(formatFEN[3])))));
            else
                switch (char.ToLower(board[sPos]))
                {
                    case 'n':
                                bool[] knightLegalMoves = Functions.LegalKnightMoves(distanceToEdge);
                                for(int i = 0; i < 8; i++)
                                    if(knightLegalMoves[i])
                                        if (sPos + nMoves[i] == ePos && (!(char.IsLower(board[sPos]) && char.IsLower(board[ePos]))
                                            || (char.IsUpper(board[sPos]) && char.IsUpper(board[ePos]))))
                                            return true;
                                    
                                break;
                    case 'b':
                        for(int i = 0; i < 4; i++)
                        {
                            int testTile = sPos;
                            for(int y = 0; y < distanceToEdge[i]; y++)
                            {
                                testTile += bMoves[i];
                                if (StopCheck(board, sPos, testTile, ePos))
                                    break;
                                if (testTile == ePos)
                                    return true;
                            }
                        }
                        return false;
                    case 'r':
                        for(int i = 4; i < 8; i++)
                        {
                            int testTile = sPos;
                            for(int y = 0; y < distanceToEdge[i]; y++)
                            {
                                testTile += rMoves[i-4];
                                if (StopCheck(board, sPos, testTile, ePos))
                                    break;
                                if (testTile == ePos)
                                    return true;
                            }
                        }
                        return false;
                    case 'q':
                        for(int i = 0; i < 8; i++)
                        {
                            int testTile = sPos;
                            for(int y = 0; y < distanceToEdge[i]; y++)
                            {
                                testTile += qkMoves[i];
                                if (StopCheck(board, sPos, testTile, ePos))
                                    break;
                                if (testTile == ePos)
                                    return true;
                            }
                        }
                        return false;
                    case 'k':
                        if((!(distanceToEdge[7] == 0 && (qkMoves[0] == (ePos - sPos)
                                                      || qkMoves[3] == (ePos - sPos)
                                                      || qkMoves[7] == (ePos - sPos))))
                        && (!(distanceToEdge[5] == 0 && (qkMoves[1] == (ePos - sPos)
                                                      || qkMoves[2] == (ePos - sPos)
                                                      || qkMoves[5] == (ePos - sPos))))
                        && qkMoves.Contains(ePos - sPos) && !checkBoard[ePos]
                        && ((char.IsLower(board[sPos]) && !char.IsLower(board[ePos]))
                            || (char.IsUpper(board[sPos]) && !char.IsUpper(board[ePos]))))
                            return true;
                        else
                            return(ePos == sPos + 2 && (!checkBoard[sPos]) && (!checkBoard[sPos+1]) && (!checkBoard[sPos+2]) 
                                && ((sPos == Functions.GenerateBoard(Graphics.mainFEN).IndexOf('k') && formatFEN[2].Contains('k')) 
                                    || (sPos == Functions.GenerateBoard(Graphics.mainFEN).IndexOf('K') && formatFEN[2].Contains('K')))) 
                            || (ePos == sPos - 2 && (!checkBoard[sPos]) && (!checkBoard[sPos-1]) && (!checkBoard[sPos-2])
                                && ((sPos == Functions.GenerateBoard(Graphics.mainFEN).IndexOf('k') && formatFEN[2].Contains('q')) 
                                    || (sPos == Functions.GenerateBoard(Graphics.mainFEN).IndexOf('K') && formatFEN[2].Contains('Q'))));
                }
            return false;
        }
        static bool StopCheck(List<char> board, int sPos, int testTile, int ePos)
        {
            return(char.IsLower(board[sPos]) && char.IsLower(board[testTile]))
                || (char.IsUpper(board[sPos]) && char.IsUpper(board[testTile]))
                || (board[testTile] != ' ' && testTile != ePos);
        }
        static bool CheckMateChecker(List<char> board, string[] formatFEN, bool[] checks)
        {
            int sPos = board.IndexOf(formatFEN[1] == "w" ? 'k' : 'K');
            foreach (int d in qkMoves)
                if (sPos + d > -1 && sPos + d < 64 && IsValid(sPos, sPos + d, board, checks, formatFEN))
                    return false;
            if(!checks[sPos])
                return StaleMateChecker(board, formatFEN, checks);
            char[] testBoard = board.ToArray();
            for (int i = 0; i < 64; i++)
            {
                if (Functions.IsEnemy(formatFEN[1] , board[i]) || board[i] == ' ')
                    continue;
                int[] distanceToEdge = Functions.DistanceToEdge(i);
                    if (board[i] == 'p')
                    {
                        foreach (int m in pMoves[1])
                        {
                            if ((m == -16 && (i < 48 || board[i - 16] != ' ' || board[i - 8] != ' '))
                            || (m == -8  && board[i - 8] != ' ')
                            || ((m == -9 && distanceToEdge[7] == 0) || (m == -7 && distanceToEdge[5] == 0)
                            && !char.IsUpper((formatFEN[3] != "-" && i + m + 8 == Functions.TileToNum(formatFEN[3])) ? board[i + m + 8] : board[i + m])))
                                continue;
                            testBoard[i + m] = 'p';
                            if(formatFEN[3] != "-" && i + m + 8 == Functions.TileToNum(formatFEN[3]))
                                testBoard[i + m + 8] = ' ';
                            testBoard[i] = ' ';
                            if (!checkChecker(testBoard.ToList(), formatFEN[1] )[Array.IndexOf(testBoard, 'k')])
                                return false;
                            testBoard = board.ToArray();
                        }
                    }
                    else if (board[i] == 'P')
                    {
                        foreach (int m in pMoves[0])
                        {
                            if ((m == 16 && (i > 15 || board[i + 16] != ' ' || board[i + 8] != ' '))
                            || (m == 8  && board[i + 8] != ' ')
                            || ((m == 7 && distanceToEdge[7] == 0) || (m == 9 && distanceToEdge[5] == 0)
                            || !char.IsLower((formatFEN[3] != "-" && i + m - 8 == Functions.TileToNum(formatFEN[3])) ? board[i + m - 8] : board[i + m])))
                                continue;
                            testBoard[i + m] = 'P';
                            if(formatFEN[3] != "-" && testBoard[i + m - 8] == Functions.TileToNum(formatFEN[3]))
                                testBoard[i + m - 8] = ' ';
                            testBoard[i] = ' ';
                            if (!checkChecker(testBoard.ToList(), formatFEN[1])[Array.IndexOf(testBoard, 'K')])
                                return false;
                            testBoard = board.ToArray();
                        }
                    }
                    else
                        switch (char.ToLower(board[i]))
                        {
                            case 'n':
                                bool[] knightLegalMoves = Functions.LegalKnightMoves(distanceToEdge);
                                for(int x = 0; x < 8; x++)
                                    if(knightLegalMoves[x])
                                    {
                                        testBoard[i + nMoves[i]] = testBoard[i];
                                        testBoard[i] = ' ';
                                        if (!checkChecker(testBoard.ToList(), formatFEN[1])[Array.IndexOf(testBoard, char.IsLower(board[i]) ? 'k' : 'K')])
                                            return false;
                                        testBoard = board.ToArray();
                                    }
                                break;
                            case 'b':
                                for(int x = 0; x < 4; x++)
                                {
                                    int testTile = i;
                                    for(int y = 0; y < distanceToEdge[x]; y++)
                                    {
                                        testTile += bMoves[x];
                                        if ((char.IsLower(board[i]) && char.IsLower(board[testTile]))
                                        || (char.IsUpper(board[i]) && char.IsUpper(board[testTile])))
                                            break;
                                        else
                                        {
                                            testBoard[testTile] = testBoard[i];
                                            testBoard[i] = ' ';
                                            if (!checkChecker(testBoard.ToList(), formatFEN[1])[Array.IndexOf(testBoard, char.IsLower(board[i]) ? 'k' : 'K')])
                                                return false;
                                            testBoard = board.ToArray();
                                        }
                                            if (board[testTile] != ' ')
                                            break;
                                    }
                                }
                                break;
                            case 'r':
                                for(int x = 4; x < 8; x++)
                                {
                                    int testTile = i;
                                    for(int y = 0; y < distanceToEdge[x]; y++)
                                    {
                                        testTile += rMoves[x - 4];
                                        if ((char.IsLower(board[i]) && char.IsLower(board[testTile]))
                                        || (char.IsUpper(board[i]) && char.IsUpper(board[testTile])))
                                            break;
                                        else
                                        {
                                            testBoard[testTile] = testBoard[i];
                                            testBoard[i] = ' ';
                                            if (!checkChecker(testBoard.ToList(), formatFEN[1])[Array.IndexOf(testBoard, char.IsLower(board[i]) ? 'k' : 'K')])
                                                return false;
                                            testBoard = board.ToArray();
                                        }
                                        if (board[testTile] != ' ')
                                            break;
                                    }
                                }
                                break;
                            case 'q':
                                for(int x = 0; x < 8; x++)
                                {
                                    int testTile = i;
                                    for(int y = 0; y < distanceToEdge[x]; y++)
                                    {
                                        testTile += qkMoves[x];
                                        if ((char.IsLower(board[i]) && char.IsLower(board[testTile]))
                                        || (char.IsUpper(board[i]) && char.IsUpper(board[testTile])))
                                            break;
                                        else
                                        {
                                            testBoard[testTile] = testBoard[i];
                                            testBoard[i] = ' ';
                                            if (!checkChecker(testBoard.ToList(), formatFEN[1])[Array.IndexOf(testBoard, char.IsLower(board[i]) ? 'k' : 'K')])
                                                return false;
                                            testBoard = board.ToArray();
                                        }
                                        if (board[testTile] != ' ')
                                            break;
                                    }
                                }
                                break;
                        }
            }
            return true;
        }
        static bool[] checkChecker(List<char> board, string onTurn)
        {
            bool[] checks = new bool[64];
            for (int i = 0; i < 64; i++)
            {
                if ((onTurn == "w" && char.IsLower(board[i]))
                || (onTurn == "b" && char.IsUpper(board[i]))
                || board[i] == ' ')
                    continue;
                int[] distanceToEdge = Functions.DistanceToEdge(i);
                    if (board[i] == 'p' &&  i > 7)
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
                        switch (char.ToLower(board[i]))
                        {
                            case 'n':
                                bool[] knightLegalMoves = Functions.LegalKnightMoves(distanceToEdge);
                                for(int x = 0; x < 8; x++)
                                    if(knightLegalMoves[x])
                                        checks[i + nMoves[x]] = true;
                                    
                                break;
                            case 'b':
                                for(int x = 0; x < 4; x++)
                                {
                                    int testTile = i;
                                    for(int y = 0; y < distanceToEdge[x]; y++)
                                    {
                                        testTile += bMoves[x];
                                        checks[testTile] = true;
                                        if (board[testTile] != ' ')
                                            break;
                                    }
                                }
                                break;
                            case 'r':
                                for(int x = 4; x < 8; x++)
                                {
                                    int testTile = i;
                                    for(int y = 0; y < distanceToEdge[x]; y++)
                                    {
                                        testTile += rMoves[x - 4];
                                        checks[testTile] = true;
                                        if (board[testTile] != ' ')
                                            break;
                                    }
                                }
                                break;
                            case 'q':
                                for(int x = 0; x < 8; x++)
                                {
                                    int testTile = i;
                                    for(int y = 0; y < distanceToEdge[x]; y++)
                                    {
                                        testTile += qkMoves[x];
                                        checks[testTile] = true;
                                        if (board[testTile] != ' ')
                                            break;
                                    }
                                }
                                break;
                            case 'k':
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
        static bool StaleMateChecker(List<char> mainBoard, string[] formatFEN, bool[] checks)
        {
            for (int i = 0; i < 64; i++)
            {
                if (Functions.IsEnemy(formatFEN[1], mainBoard[i]) || mainBoard[i] == ' ')
                    continue;
                int[] distanceToEdge = Functions.DistanceToEdge(i);
                if (mainBoard[i] == 'p')
                    foreach (int m in pMoves[1])
                        if(IsValid(i, i + m, mainBoard, checks, formatFEN))
                            return false;
                if (mainBoard[i] == 'P')
                    foreach (int x in pMoves[0])
                        if(IsValid(i, i + x, mainBoard, checks, formatFEN))
                            return false;
                switch (char.ToLower(mainBoard[i]))
                    {
                        case 'n':
                            bool[] knightLegalMoves = Functions.LegalKnightMoves(distanceToEdge);
                            for(int x = 0; x < 8; x++)
                                if(knightLegalMoves[x] && IsValid(i, i + nMoves[x], mainBoard, checks, formatFEN))
                                    return false;
                            break;
                        case 'b':
                                for(int x = 0; x < 4; x++)
                                {
                                    int testTile = i;
                                    for(int y = 0; y < distanceToEdge[x]; y++)
                                    {
                                        testTile += bMoves[x];
                                        if ((char.IsLower(mainBoard[i]) && char.IsLower(mainBoard[testTile]))
                                        || (char.IsUpper(mainBoard[i]) && char.IsUpper(mainBoard[testTile])))
                                            break;
                                        else
                                            if(IsValid(i, testTile, mainBoard, checks, formatFEN))
                                                return false;
                                        if (mainBoard[testTile] != ' ')
                                            break;
                                    }
                                }
                                break;
                        case 'r':
                                for(int x = 4; x < 8; x++)
                                {
                                    int testTile = i;
                                    for(int y = 0; y < distanceToEdge[x]; y++)
                                    {
                                        testTile += rMoves[x - 4];
                                        if ((char.IsLower(mainBoard[i]) && char.IsLower(mainBoard[testTile]))
                                        || (char.IsUpper(mainBoard[i]) && char.IsUpper(mainBoard[testTile])))
                                            break;
                                        else
                                            if(IsValid(i, testTile, mainBoard, checks, formatFEN))
                                                return false;
                                        if (mainBoard[testTile] != ' ')
                                            break;
                                    }
                                }
                                break;
                        case 'q':
                                for(int x = 0; x < 8; x++)
                                {
                                    int testTile = i;
                                    for(int y = 0; y < distanceToEdge[x]; y++)
                                    {
                                        testTile += qkMoves[x];
                                        if ((char.IsLower(mainBoard[i]) && char.IsLower(mainBoard[testTile]))
                                        || (char.IsUpper(mainBoard[i]) && char.IsUpper(mainBoard[testTile])))
                                            break;
                                        else
                                            if(IsValid(i, testTile, mainBoard, checks, formatFEN))
                                                return false;
                                        if (mainBoard[testTile] != ' ')
                                            break;
                                    }
                                }
                                break;
                        }
            }
            staleMate = true;
            return true;
        }

       
    }
}
