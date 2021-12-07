using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class BoonOfSwarmingBedevilment {

		[SpiritCard("Boon of Swarming Bedevilment",0,Element.Air,Element.Water,Element.Animal), Fast, AnotherSpirit]
		static public async Task ActAsync(TargetSpiritCtx ctx ) {
			// for the rest of this turn,
			// each of target Spirit's presence grants Defend 1 in its land.
			// !!! re write this to add the defence now, then drag it around if/when the presence is moved
			ctx.GameState.PreRavaging.ForRound.Add( ( gs, args ) => {
				foreach(var space in ctx.Other.Presence.Placed)
					ctx.OtherCtx.Target( space ).Defend( 1 );
			} );

			// Target Spirit may Push up to 1 of their presence.
			await ctx.OtherCtx.Presence.PushUpTo1();
		}

	}


}
