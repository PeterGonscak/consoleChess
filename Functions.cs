using System;
using System.Collections.Generic;
using System.Linq;

namespace test
{
    public class Functions 
    {
        public readonly Dictionary<char, int> pieceValues = new Dictionary<char, int>(){
            {'p', 1}, {'P', -1},
            {'n', 3}, {'N', -3},
            {'b', 3}, {'B', -3},
            {'r', 5}, {'R', -5},
            {'q', 9}, {'Q', -9},
            {'k', 0}, {'K', 0},
            {' ', 0}
        };
        public readonly Dictionary<char, int> tileValues = new Dictionary<char, int>(){
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
        public readonly int [][] pMoves = new int [2][] {
            new int[4] { 7, 8, 9, 16 }, 
            new int[4] { -7, -8, -9, -16 }
        };
        public readonly int[][] nMoves = new int[4][] { 
            new int[2] { -10, 6 }, 
            new int[2] { -17, 15 }, 
            new int[2] { -6, 10 }, 
            new int[2] { -15, 17 } 
        };
        public readonly int[] bMoves = new int[4] { -9, -7, 7, 9};
        public readonly int[] rMoves = new int[4] {-8, -1, 1, 8 };
        public readonly int[] qkMoves = new int[8] { -9, -7, 7, 9, -8, -1, 1, 8 };
        
        public bool isEnemy(string onTurn, char piece)
        {
            return (onTurn == "b" && char.IsLower(piece))
                || (onTurn == "w" && char.IsUpper(piece));
        }
        public bool isFriend(string onTurn, char piece)
        {
            return (onTurn == "w" && char.IsLower(piece))
                || (onTurn == "b" && char.IsUpper(piece));
        }
    }
}