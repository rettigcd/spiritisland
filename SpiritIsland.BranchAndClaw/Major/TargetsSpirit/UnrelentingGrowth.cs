using System;

namespace SpiritIsland.BranchAndClaw;

public class UnrelentingGrowth {

	[MajorCard( "Unrelenting Growth", 4, Element.Sun, Element.Fire, Element.Water, Element.Plant )]
	[Slow]
	[AnySpirit]
	static public async Task ActAsync( TargetSpiritCtx ctx ) {

		TargetSpaceCtx toCtx = await AddPresenceAndWilds( ctx.OtherCtx );

		// if you have 3 sun, 3 plant
		if(await ctx.YouHave( "3 sun,3 plant" )) {
			// in that land add 1 additional wilds 
			await toCtx.Wilds.Add(1);
			// and remove 1 blight.
			var blight = toCtx.Blight;
			if(blight.Any) await toCtx.RemoveBlight();

			// Target Spirit gains a power card.
			await ctx.Self.Draw();
		}

	}

	static async Task<TargetSpaceCtx> AddPresenceAndWilds( SelfCtx ctx ) {

		// target spirit adds 2 presence and 1 wilds to a land at range 1

		// Select destination
		var to = await ctx.Self.SelectDestinationWithinRange( new TargetCriteria( 1 ), true );

		// add wilds
		var toCtx = ctx.Target( to );
		await toCtx.Wilds.Add(1);

		// Add presence
		for(int i = 0; i < 2; ++i) {
			var from = await ctx.Self.SelectMovablePresence();
			if(from !=null)
				await ctx.Self.Presence.Place( from, to );
		}

		return toCtx;
	}

}