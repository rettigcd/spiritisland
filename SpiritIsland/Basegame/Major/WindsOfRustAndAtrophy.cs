﻿
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class WindsOfRustAndAtrophy {

		[MajorCard("Winds of Rust and Atrophy",3,Speed.Fast,Element.Air,Element.Moon,Element.Animal)]
		[FromSacredSite(3)]
		static public async Task ActAsync(ActionEngine engine,Space target ) {
			var (self, _) = engine;
			await ApplyEffect( engine, target );

			// if you have 3 air 3 water 2 animal, repeat this power
			if(self.Elements.Contains("3 air,2 water,2 animal" )) {
				var secondTarget = await engine.TargetSpace(From.SacredSite,3,Target.Any);
				await ApplyEffect( engine, secondTarget);
			}
		}

		static async Task ApplyEffect( ActionEngine engine, Space target ) {
			var (_,gs) = engine;
			// 1 fear and defend 6
			engine.AddFear( 1 );
			gs.Defend( target, 6 );

			// replace 1 city with 1 town OR 1 town with 1 explorer
			var grp = gs.InvadersOn( target );
			var options = grp.FilterBy( Invader.City, Invader.Town );
			var invader = await engine.Self.SelectInvader( "Select invader to downgrade (city>town,town>explorer)", options );
			if(invader.Generic == Invader.City) {
				gs.Adjust( target, invader, -1 );
				gs.Adjust( target, InvaderSpecific.Town, 1 );
			} else if(invader.Generic == Invader.Town) {
				gs.Adjust( target, invader, -1 );
				gs.Adjust( target, InvaderSpecific.Explorer, 1 );
			}
		}
	}
}
