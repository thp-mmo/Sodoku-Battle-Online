using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class Room
    {
        public Player Player1 { get; set; }

        public Player Player2 { get; set; }

        public Board Board { get; set; }
        public Room()
        {
            Player1 = new Player();
            Player2 = new Player();
            Board = new Board();
        }

    }
}
