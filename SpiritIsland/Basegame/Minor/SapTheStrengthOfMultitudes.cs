using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class SapTheStrengthOfMultitudes {

		// !!! need a special targetting method
		// if you have 1 air, increate this power's range by 1


		[MinorCard( "Sap the Strength of Multitudes", 0, Speed.Fast, "water, animal" )]
		[FromPresence( 0 )]
		static public Task ActAsync( TargetSpaceCtx ctx) {
			var target = ctx.Target;
			// defend 5
			ctx.GameState.Defend(target,5);
			return Task.CompletedTask;
		}


	}
}
