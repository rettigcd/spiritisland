﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class BlazingRenewal {

		[MajorCard("Blazing Renewal",5,Speed.Fast,Element.Fire,Element.Earth,Element.Plant)]
		[TargetSpirit]
		static public async Task ActAsync(ActionEngine engine, Spirit target ) {

			// target spirit adds 2 of their destroyed presence
			var targetEngine = new ActionEngine( target, engine.GameState );
			int max = await targetEngine.SelectNumber("Select # of destroyed presence to return to board", target.Presence.Destroyed );
			if(max==0) return;

			// into a single land, up to range 2 from your presence.
			// Note - Jonah says it is the originators power and range and decision, not the targets
			var landTarget = await engine.Api.TargetSpace( engine, From.Presence, 2, Target.Any );

			// Add it!
			for(int i=0;i<max;++i)
				target.Presence.PlaceFromBoard(Track.Destroyed,landTarget);

			// if any presene was added, 2 damage to each town/city in that land.
			var grp = engine.GameState.InvadersOn(landTarget);
			grp.ApplyDamageToEach(2,Invader.Town,Invader.City);

			// if you have 3 fire, 3 earth , 2 plant, 4 damage in that land
			if(engine.Self.Elements.Contains("3 fire,3 earth,2 plant"))
				await grp.ApplySmartDamageToGroup(4);


		}
	}

	class Entwined : PowerCardApi {

		readonly Spirit[] spirits;

		public Entwined(params Spirit[] spirits){
			this.spirits = spirits;
		}

		public override IEnumerable<Space> GetTargetOptions( Spirit _, From sourceEnum, Terrain? sourceTerrain, int range, Target filterEnum, GameState gameState ) {
			return spirits
				.SelectMany(Spirit=>base.GetTargetOptions(Spirit,sourceEnum,sourceTerrain,range,filterEnum,gameState))
				.Distinct();
		}
	}
}
