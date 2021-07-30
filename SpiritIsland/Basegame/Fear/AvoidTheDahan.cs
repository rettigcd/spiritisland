using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Basegame {

    class AvoidTheDahan {

        [FearLevel(1, "Invaders do not Explore into lands with at least 2 Dahan." )]
        public static void M1() { }

        [FearLevel( 2, "Invaders do not Build in lands where Dahan outnumber Town / City." )]
        public static void M2() { }

        [FearLevel(3, "Invaders do not Build in lands with Dahan." )]
        public static void M3() { }
    }

}

