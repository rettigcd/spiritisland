﻿using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class FlowLikeWaterReachLikeAir {

		[MajorCard("Flow Like Water, Reach Like Air",2,Element.Air,Element.Water), Fast, AnySpirit]
		static public async Task ActAsync( TargetSpiritCtx ctx ) {

			// target spirit gets +2 range with all Powers.
			TargetLandApi.ScheduleRestore( ctx.OtherCtx );
			TargetLandApi.ExtendRange( ctx.Other, 2 );

			// Target spirit may push 1 of their presence to an adjacent land
			var sourceCtx = await ctx.OtherCtx.TargetDeployedPresence("Push Presence (bringing up to 2 explorers, 2 towns, 2 dahan)");

			// #pushpresence
			var destination = await sourceCtx.Self.Action.Decision( new Decision.Presence.Push(
				"Move Presence + up to 2 explorers,towns,dahan to",
				sourceCtx.Space,
				sourceCtx.Adjacent
			));

			// move presence
			ctx.Other.Presence.Move( sourceCtx.Space, destination);

			var mover = new TokenPusher_FixedDestination(ctx.OtherCtx.Target(sourceCtx.Space), destination);
			// bringing up to 2 explorers, 2 towns and 2 dahan along with it.
			mover.AddGroup(2,Invader.Explorer);
			mover.AddGroup(2,Invader.Town);
			mover.AddGroup(2,TokenType.Dahan);

			// if you hvae 2 air, 2 water, the moved presence may also bring along up to 2 cities and up to 2 blight.
			if(await ctx.YouHave( "2 air,2 water" )) {
				mover.AddGroup(2,Invader.City);
				mover.AddGroup(2,TokenType.Blight.Generic);
			}
			
			await mover.MoveUpToN();

		}

	}

}
