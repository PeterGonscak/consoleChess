using System;
using System.Collections.Generic;
using System.Linq;

namespace consoleChess
{
    /// <summary>
    /// Dictionarys and functions.
    /// </summary>
    public static class Functions
    {
        /// <summary>
        /// Letter to its value.
        /// </summary>
        public static readonly Dictionary<char, int> pieceValues = new Dictionary<char, int>(){
            {'p', 1}, {'P', -1},
            {'n', 3}, {'N', -3},
            {'b', 3}, {'B', -3},
            {'r', 5}, {'R', -5},
            {'q', 9}, {'Q', -9},
            {'k', 0}, {'K', 0},
            {' ', 0}
        };
        /// <summary>
        /// column or row to value.
        /// </summary>
        public static readonly Dictionary<char, int> tileValues = new Dictionary<char, int>(){
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
        /// <summary>
        /// value to row
        /// </summary>
        public static readonly Dictionary<int, char> numValues = new Dictionary<int, char>(){
            {0, 'a'},
            {1, 'b'},
            {2, 'c'},
            {3, 'd'},
            {4, 'e'},
            {5, 'f'},
            {6, 'g'},
            {7, 'h'}
        };
        /// <summary>
        /// value to column
        /// </summary>
        public static readonly Dictionary<int, char> rowValues = new Dictionary<int, char>(){
            {56, '1'},
            {48, '2'},
            {40, '3'},
            {32, '4'},
            {24, '5'},
            {16, '6'},
            {8, '7'},
            {0, '8'}
        };
        /// <summary>
        /// checks if piece belongs to player off turn.
        /// </summary>
        /// <param name="onTurn"> player on turn.</param>
        /// <param name="piece"> piece to check.</param>
        /// <returns> true if is enemy </returns>
        public static bool IsEnemy(string onTurn, char piece)
        {
            return (onTurn == "w" && char.IsLower(piece))
                || (onTurn == "b" && char.IsUpper(piece));
        }
        /// <summary>
        /// checks if piece belongs to player on turn.
        /// </summary>
        /// <param name="onTurn"> player on turn.</param>
        /// <param name="piece"> piece to check.</param>
        /// <returns> true if is friend </returns>
        public static bool IsFriend(string onTurn, char piece)
        {
            return (onTurn == "b" && char.IsLower(piece))
                || (onTurn == "w" && char.IsUpper(piece));
        }
        /// <summary>
        /// basic evaluation of board.
        /// </summary>
        /// <param name="board"> board to evaluate. </param>
        /// <returns> Evaluation </returns>
        public static int Eval(List<char> board)
        {
            int sum = 0;
            foreach (char c in board)
                sum += pieceValues[c];
            return sum;
        }
        /// <summary>
        /// Generates FEN string from board
        /// </summary>
        /// <param name="board"> board from which to create FEN</param>
        /// <param name="formatFEN"> used to add rules at the end. </param>
        /// <returns>FEN string</returns>
        public static string GenerateFEN(List<char> board, string[] formatFEN)
        {
            string FEN = "";
            int tileCount = 0;
            for (int i = 0; i < 64; i++)
            {
                if (board[i] == ' ')
                    tileCount++;
                else
                {
                    FEN += (tileCount == 0 ? "" : tileCount.ToString()) + board[i];
                    tileCount = 0;
                }
                if (i % 8 == 7)
                    {
                        FEN += (tileCount == 0 ? "" : tileCount) + (i != 63 ? "/" : "");
                        tileCount = 0;
                    }
            }
            return FEN += " " + formatFEN[1] + " " + formatFEN[2] + " " + formatFEN[3] + " " + formatFEN[4] + " " + formatFEN[5];
        }
        /// <summary>
        /// translates row and column to num.
        /// </summary>
        /// <param name="s"> row and column</param>
        /// <returns>int representation of a tile</returns>
        public static int TileToNum(string s)
        {
            return tileValues[s[0]] + tileValues[s[1]];
        }
        /// <summary>
        /// translates tile number to row and column.
        /// </summary>
        /// <param name="i"> tile number</param>
        /// <returns> string representation of num</returns>
        public static string NumToTile(int i) 
        {
            return numValues[i % 8].ToString() + rowValues[i - (i % 8)];
        }
        /// <summary>
        /// Generates board from FEN specified.
        /// </summary>
        /// <param name="FEN"> FEN string used to generate board</param>
        /// <returns> List filled with char representations of pieces </returns>
        public static List<char> GenerateBoard(string FEN)
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
        public static char SelectPiece(string onTurn)
        {
            string s = " ";
            while ((s.Length != 1 && !"nbrq".Contains(s)) || s == " ")
            {
                Console.Write("Type to which piece do you want to promote the pawn. [n / b / r / q]: ");
                s = "" + Console.ReadLine();
            }
            return char.Parse(onTurn == "b" ? s : s.ToUpper());
        }
        public static byte[][] DistanceToEdge()
        {
            byte[][] board = new byte[64][];
            for(byte i = 0; i < 64; i++)
                board[i] =  new byte[]{
                (byte)(i % 8 > (i - (i % 8)) / 8 ? (i - (i % 8)) / 8 : i % 8),
                (byte)(7 - i % 8 > (i - (i % 8)) / 8 ? (i - (i % 8)) / 8 : 7 - i % 8),
                (byte)(7 - i % 8 > 7 - ((i - (i % 8)) / 8) ?  7 - ((i - (i % 8)) / 8) : 7 - i % 8),
                (byte)(i % 8 > 7 - ((i - (i % 8)) / 8) ?  7 - ((i - (i % 8)) / 8): i % 8),
                (byte)((i - (i % 8)) / 8),
                (byte)(7 - i % 8),
                (byte)(7 - ((i - (i % 8)) / 8)),
                (byte)(i % 8)
            };
            return board;
        }
        public static bool[,] LegalKnightMoves(byte[][] dte)
        {
            bool[,] legalMoves = new bool[64,8];
            for(byte i = 0; i < 64; i++)
            {
            if(dte[i][5] != 0)
            {
                if(dte[i][5] != 1)
                {
                    if(dte[i][4] != 0)
                        legalMoves[i, 2] = true;
                    if(dte[i][6] != 0)
                        legalMoves[i, 3] = true;
                }
                if(dte[i][4] > 1)
                    legalMoves[i, 1] = true;
                if(dte[i][6] > 1)
                    legalMoves[i, 4] = true;
            }
            if(dte[i][7] != 0)
            {
                if(dte[i][7] != 1)
                {
                    if(dte[i][4] != 0)
                        legalMoves[i, 7] = true;
                    if(dte[i][6] != 0)
                        legalMoves[i, 6] = true;
                }
                if(dte[i][4] > 1)
                    legalMoves[i, 0] = true;
                if(dte[i][6] > 1)
                    legalMoves[i, 5] = true;
            }
            }
            return legalMoves;
           }
        public static Dictionary<byte, byte> flippedDirections = new Dictionary<byte, byte>(){
            {0, 2},
            {1, 3},
            {2, 0},
            {3, 1},
            {4, 6},
            {5, 7},
            {6, 4},
            {7, 5},
        };
    }
}
