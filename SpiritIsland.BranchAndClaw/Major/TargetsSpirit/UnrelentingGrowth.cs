namespace SpiritIsland.BranchAndClaw;

public class UnrelentingGrowth {

	[MajorCard( "Unrelenting Growth", 4, Element.Sun, Element.Fire, Element.Water, Element.Plant ),Slow,AnySpirit]
	[Instructions( "Target Spirit adds 2 Presence and 1 Wilds to a land at 1 Range. -If you have- 3 Sun, 3 Plant: In that land, add 1 additional Wilds and remove 1 Blight. Target Spirit gains a Power Card." ), Artist( Artists.JoshuaWright )]
	static public async Task ActAsync( TargetSpiritCtx ctx ) {

		TargetSpaceCtx? toCtx = await AddPresenceAndWilds( ctx.Other );
		if(toCtx is null) return;

		// if you have 3 sun, 3 plant
		if(await ctx.YouHave( "3 sun,3 plant" )) {
			// in that land add 1 additional wilds 
			await toCtx.Wilds.AddAsync(1);
			// and remove 1 blight.
			var blight = toCtx.Blight;
			if(blight.Any) await toCtx.RemoveBlight();

			// Target Spirit gains a power card.
			await ctx.Self.Draw.Card();
		}

	}

	static async Task<TargetSpaceCtx?> AddPresenceAndWilds( Spirit self ) {

		// target spirit adds 2 presence and 1 wilds to a land at range 1

		// Select destination
		var options = self.FindSpacesWithinRange(new TargetCriteria(1))
			.Where( self.Presence.CanBePlacedOn )
			.ToArray();
		var to = await self.Select( new A.SpaceDecision( "Where would you like to place your presence?", options, Present.Always ) );
		if(to is null) return null;

		// add wilds
		var toCtx = self.Target( to );
		await toCtx.Wilds.AddAsync(1);

		// Add presence
		for(int i = 0; i < 2; ++i) {
			var from = await self.SelectAlways(Prompts.SelectPresenceTo(), self.DeployablePresence());
			await from.MoveToAsync( to );
		}

		return toCtx;
	}

}