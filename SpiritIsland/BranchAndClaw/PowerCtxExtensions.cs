using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {
	public static class PowerCtxExtensions {
		
		static public GameState_BranchAndClaw BAC( this GameState gs ) => (GameState_BranchAndClaw)gs;

		static public async Task<Space[]> PowerPushUpToNBeasts( this PowerCtx ctx, Space source, int beastsToPush ) {
			HashSet<Space> pushedToLands = new HashSet<Space>();
			beastsToPush = System.Math.Min( beastsToPush, ctx.GameState.BAC().Beasts.GetCount( source ) );
			while(0 < beastsToPush) {
				Space destination = await ctx.Self.Action.Choose( new PushBeastDecision(
					source
					, TargetSpaceCtx.PowerAdjacents( source )
					, Present.Done
				) );
				if(destination == null) break;
				pushedToLands.Add( destination );
				await ctx.GameState.BAC().Beasts.Move( source, destination );
				--beastsToPush;
			}
			return pushedToLands.ToArray();
		}

		static public async Task GatherUpToNBeasts( this IMakeGamestateDecisions eng, Space target, int beastsToGather ) {
			int gathered = 0;
			var neighborsWithBeasts = target.Adjacent.Where( eng.GameState.BAC().Beasts.AreOn ).ToArray();
			while(gathered < beastsToGather && neighborsWithBeasts.Length > 0) {
				var source = await eng.Self.Action.Choose( new GatherBeastsFromDecision( beastsToGather - gathered, target, neighborsWithBeasts, Present.Done ) );
				if(source == null) break;

				await eng.GameState.BAC().Beasts.Move( source, target );

				++gathered;
				neighborsWithBeasts = target.Adjacent.Where( eng.GameState.Dahan.AreOn ).ToArray();
			}

		}


	}

	public class PushBeastDecision : SelectAdjacentDecision {

		public PushBeastDecision( Space source, IEnumerable<Space> destinationOptions, Present present )
			: base( "Select destination for beasts", source, GatherPush.Push, destinationOptions, present ) {
			Source = source;
		}

		public Space Source { get; }
	}

	public class GatherBeastsFromDecision : SelectAdjacentDecision {
		public GatherBeastsFromDecision( int remaining, Space to, IEnumerable<Space> spaces, Present present = Present.IfMoreThan1 )
			: base( $"Gather Beasts ({remaining} remaining)", to, GatherPush.Gather, spaces, present ) {
		}
	}


}
