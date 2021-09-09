using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class FlowLikeWaterReachLikeAir {

		[MajorCard("Flow Like Water, Reach Like Air",2,Speed.Fast,Element.Air,Element.Water)]
		[TargetSpirit]
		static public async Task ActAsync( TargetSpiritCtx ctx ) {
			// target spirit gets +2 range with all Powers.
			TargetLandApi.ScheduleRestore( ctx );
			TargetLandApi.ExtendRange( ctx.Self, 2 );

			// Target spirit may push 1 of their presence to an adjacent land
			var source = await ctx.Target.Action.Decide(new SelectDeployedPresence("Push Presence (bringing up to 2 explorers, 2 towns, 2 dahan)",ctx.Target));
			var landCtx = new TargetSpaceCtx(ctx.Target,ctx.GameState,source);

			var destination = await landCtx.Self.Action.Decide( new SelectAdjacentDecision(
				"Move Presence + up to 2 explorers,towns,dahan to",
				source,
				GatherPush.Push,
				landCtx.PowerAdjacents(),
				Present.Always
			));

			// move presence
			ctx.Target.Presence.Move(source,destination);

			var mover = new TokenMover(ctx.TargetCtx,source,destination);
			// bringing up to 2 explorers, 2 towns and 2 dahan along with it.
			mover.AddGroup(2,Invader.Explorer);
			mover.AddGroup(2,Invader.Town);
			mover.AddGroup(2,TokenType.Dahan);

			// if you hvae 2 air, 2 water, the moved presence may also bring along up to 2 cities and up to 2 blight.
			if(ctx.Target.Elements.Contains( "" )) {
				mover.AddGroup(2,Invader.City);
				mover.AddGroup(2,TokenType.Blight.Generic);
			}
			
			await mover.MoveUpToN();

		}

	}

}
