using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class BoonOfSwarmingBedevilment {

		[SpiritCard("Boon of Swarming Bedevilment",0,Element.Air,Element.Water,Element.Animal), Fast, AnotherSpirit]
		static public async Task ActAsync(TargetSpiritCtx ctx ) {
			// for the rest of this turn,
			// each of target Spirit's presence grants Defend 1 in its land.
			ctx.GameState.PreRavaging.ForThisRound( ( gs, args ) => {
				foreach(var space in ctx.Other.Presence.Placed)
					ctx.OtherCtx.Target( space ).Defend( 1 );
			} );

			// Target Spirit may Push up to 1 of their presence.
			await PushPresence( ctx.OtherCtx );
		}

		static async Task PushPresence( SpiritGameStateCtx ctx ) {
			// Select source
			var source = await ctx.Self.Action.Decision( Decision.Presence.Deployed.SourceForPlacing( ctx.Self ) );
			var sourceCtx = ctx.Target( source );
			// Select destination
            var destination = await sourceCtx.Self.Action.Decision( new Decision.Presence.Push( "Push Presence to", sourceCtx.Space, sourceCtx.Adjacent ));
			sourceCtx.Presence.Move( source, destination );
		}
	}


}
