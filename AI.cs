using System;
using System.Collections.Generic;
using System.Linq;

namespace test
{
    public class AI
    {
        List<char> board = Functions.GenerateBoard(Graphics.mainFEN);
        string onTurn;
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
            {' ', 0},
            {'p', 1},
            {'n', 2},
            {'b', 3},
            {'r', 4},
            {'q', 5},
            {'k', 6},
            {'P', 7},
            {'N', 8},
            {'B', 9},
            {'R', 10},
            {'Q', 11},
            {'K', 12}
        };
        public AI(string onTurn)
        {
            this.onTurn = onTurn;
        }

        public string PlayMove()
        {
            return "";
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