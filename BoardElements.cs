using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace consoleChess
{
    public struct Tile
    {
        public byte type;

        public byte position;

        public bool isWhite;

        public Tile(byte type, byte position, bool isWhite)
        {
            this.type = type;
            this.position = position;
            this.isWhite = isWhite;
        }
        
    }
    /*
    public class WhitePawn : Tile
    {

    }

    public class BlackPawn : Tile
    {

    }

    public class Knight : Tile
    {

    }

    public class Bishop : Tile
    {

    }

    public class Rook : Tile
    {

    }

    public class Queen : Tile
    {

    }
    public class King : Tile
    {

    }
    */
}
