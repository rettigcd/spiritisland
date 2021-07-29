using SpiritIsland.Core;

namespace SpiritIsland.Base {
	class CallOfTheDahanWays {

		[MinorCard("Call of the Dahan Ways",1,Speed.Slow,Element.Moon,Element.Water,Element.Animal)]
		[FromPresence(1,Filter.Dahan)]
		static public void Act(ActionEngine eng,Space target){
			var grp = eng.GameState.InvadersOn(target);

			// if you have 2 moon, you may instead replace 1 town with 1 dahan
			if(grp.HasTown && 2 <= eng.Self.Elements[ Element.Moon ]) {
				eng.GameState.Adjust(target,Invader.Town,-1);
				eng.GameState.AddDahan(target,1);
            } else if( grp.HasExplorer) {
				// replace 1 explorer with 1 dahan
				eng.GameState.Adjust( target, Invader.Explorer, -1 );
				eng.GameState.AddDahan( target, 1 );
			}

		}

	}
}
