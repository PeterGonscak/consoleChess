using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace consoleChess
{
    public struct Board
    {
        public Tile[] board = new Tile[64];
        public List<Tile> whitePieces = new();
        public List<Tile> blackPieces = new();


        public byte whiteOnTurn;

        public bool whiteCastleKing;
        public bool whiteCastleQueen;
        public bool blackCastleKing;
        public bool blackCastleQueen;

        public byte enPassantSquare;

        public byte halfMoveClock;
        public Int16 fullMoveClock;

        public byte movesSinceCapture;

        public Board(byte whiteOnTurn, bool K, bool Q, bool k, bool q, byte enPassantSquare, byte halfMoveClock, Int16 fullMoveClock, byte movesSinceCapture)
        {
            this.whiteOnTurn = whiteOnTurn;
            whiteCastleKing = K;
            whiteCastleQueen = Q;
            blackCastleKing = k;
            blackCastleQueen = q;
            this.enPassantSquare = enPassantSquare;
            this.halfMoveClock = halfMoveClock;
            this.fullMoveClock = fullMoveClock;
            this.movesSinceCapture = movesSinceCapture;
        }

        public Board Clone()
        {
            Board copy = new Board(
            this.whiteOnTurn,
            this.whiteCastleKing,
            this.whiteCastleQueen,
            this.blackCastleKing,
            this.blackCastleQueen,
            this.enPassantSquare,
            this.halfMoveClock,
            this.fullMoveClock,
            this.movesSinceCapture);
            copy.board = this.board.Clone() as Tile[];
            copy.whitePieces = this.whitePieces.ToArray().ToList();
            copy.blackPieces = this.blackPieces.ToArray().ToList();
            return copy;
        }

        
    }
        
}
