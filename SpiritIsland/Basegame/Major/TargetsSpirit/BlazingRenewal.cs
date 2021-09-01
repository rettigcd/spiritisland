﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class BlazingRenewal {

		[MajorCard("Blazing Renewal",5,Speed.Fast,Element.Fire,Element.Earth,Element.Plant)]
		[TargetSpirit]
		static public async Task ActAsync( TargetSpiritCtx ctx ) {
			var target = ctx.Target;

			// target spirit adds 2 of their destroyed presence
			int max = await target.SelectNumber("Select # of destroyed presence to return to board", target.Presence.Destroyed );
			if(max==0) return;

			// into a single land, up to range 2 from your presence.
			// Note - Jonah says it is the originators power and range and decision, not the targets
			var landTarget = await ctx.PowerTargetsSpace( From.Presence, null, 2, Target.Any );

			// Add it!
			for(int i=0;i<max;++i)
				await target.Presence.PlaceFromBoard(Track.Destroyed,landTarget, ctx.GameState);

			// if any presene was added, 2 damage to each town/city in that land.
			var grp = ctx.GameState.Invaders.On(landTarget,Cause.Power);
			await grp.ApplyDamageToEach(2,Invader.Town,Invader.City);

			// if you have 3 fire, 3 earth , 2 plant, 4 damage in that land
			if(ctx.Self.Elements.Contains("3 fire,3 earth,2 plant"))
				await grp.ApplySmartDamageToGroup(4);


		}
	}

}
