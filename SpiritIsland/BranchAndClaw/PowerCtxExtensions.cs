using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public static class PowerCtxExtensions {
		
		static public GameState_BranchAndClaw BAC( this GameState gs ) => (GameState_BranchAndClaw)gs;

		static public Task AddStrife( this TargetSpaceCtx ctx ) => ctx.Self.SelectInvader_ToStrife( ctx.Tokens );

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
