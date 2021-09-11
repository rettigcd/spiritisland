using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class FlowLikeWaterReachLikeAir {

		[MajorCard("Flow Like Water, Reach Like Air",2,Speed.Fast,Element.Air,Element.Water)]
		[TargetSpirit]
		static public async Task ActAsync( TargetSpiritCtx ctx ) {

			// target spirit gets +2 range with all Powers.
			TargetLandApi.ScheduleRestore( ctx.OtherCtx );
			TargetLandApi.ExtendRange( ctx.Other, 2 );

			// Target spirit may push 1 of their presence to an adjacent land
			var sourceCtx = await ctx.OtherCtx.TargetLandWithPresence("Push Presence (bringing up to 2 explorers, 2 towns, 2 dahan)");

			var destination = await sourceCtx.Self.Action.Decision( new Decision.AdjacentSpace(
				"Move Presence + up to 2 explorers,towns,dahan to",
				sourceCtx.Space,
				Decision.GatherPush.Push,
				sourceCtx.Adjacents,
				Present.Always
			));

			// move presence
			ctx.Other.Presence.Move( sourceCtx.Space, destination);

			var mover = new TokenMover(ctx.OtherCtx, sourceCtx.Space, destination);
			// bringing up to 2 explorers, 2 towns and 2 dahan along with it.
			mover.AddGroup(2,Invader.Explorer);
			mover.AddGroup(2,Invader.Town);
			mover.AddGroup(2,TokenType.Dahan);

			// if you hvae 2 air, 2 water, the moved presence may also bring along up to 2 cities and up to 2 blight.
			if(ctx.Other.Elements.Contains( "" )) {
				mover.AddGroup(2,Invader.City);
				mover.AddGroup(2,TokenType.Blight.Generic);
			}
			
			await mover.MoveUpToN();

		}

	}

}
