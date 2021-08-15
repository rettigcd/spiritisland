using SpiritIsland;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	[InnatePower( "Creepers Tear into Mortar", Speed.Slow )]
	[FromPresence( 0 )]
	class CreepersTearIntoMortar {

		[InnateOption( "1 moon, 2 plant" )]
		static public Task Option1Async( ActionEngine engine, Space target ) {
			return DoDamage( engine, target, 1 );
		}

		[InnateOption( "2 moon, 3 plant" )]
		static public Task Option2Async( ActionEngine engine, Space target ) {
			return DoDamage( engine, target, 2 );
		}

		[InnateOption( "3 moon, 4 plant" )]
		static public Task Option3Async( ActionEngine engine, Space target ) {
			return DoDamage(engine,target,3);
		}

		static Task DoDamage(ActionEngine engine, Space target, int damage ) {
			return engine.GameState.InvadersOn( target ).SmartDamageToTypes( damage, Invader.City, Invader.Town );
		}
	}
}