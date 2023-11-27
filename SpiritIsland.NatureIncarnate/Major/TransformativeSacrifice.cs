namespace SpiritIsland.NatureIncarnate;

public class TransformativeSacrifice {

	public const string Name = "Transformative Sacrifice";

	[MajorCard(Name,3,"moon,fire,water,plant"),Fast]
	[AnySpirit]
	[Instructions( "Target Spirit may Remove up to 3 Presence. For each removed Presence, Take a Minor Power and play it for free. -If you have- 2 moon,3 fire,2 plant: Before taking cards, they may also Remove 1 presence from their presence track to Take a Minor Power and play it." ), Artist( Artists.KatGuevara )]
	static public async Task ActAsync(TargetSpiritCtx ctx){
		bool hasElementThreshold = await ctx.OtherCtx.YouHave("2 moon,3 fire,2 plant");
		await TargetSpiritAction( ctx.OtherCtx, hasElementThreshold );
	}

	static async Task TargetSpiritAction(SelfCtx ctx, bool boostFromElementThreshold ) {
		// Target Spirit may Remove up to 3 Presence.
		int count = 3;
		int destroyed = 0;
		while(0 < count--) {
			SpaceToken spaceToken = await ctx.Self.Select( A.SpaceToken.OfDeployedPresence( $"Destroy up to {count+1} to Take Minor and Play it for free.", ctx.Self, Present.Done ) );
			if(spaceToken == null) break;
			await spaceToken.Destroy();
			++destroyed;
		}

		if( boostFromElementThreshold ){
			// Before taking cards,
			// they may also Remove 1 presence from their presence track to Take a Minor Power and play it.
			IOption presenceToRemove = await ctx.Self.SelectSourcePresence("Remove from game for additional Minor."); // Come from track or board
			if(presenceToRemove != null) {
				await ctx.Self.Presence.TakeFromAsync( presenceToRemove );
				++ctx.Self.Presence.Destroyed;
				++destroyed;
			}
		}

		// Show them all at once
		DrawCardResult result = await DrawFromDeck.DrawInner(ctx.Self,GameState.Current.MinorCards,destroyed,destroyed);
		foreach(PowerCard? card in result.SelectedCards)
			ctx.Self.PlayCard(card,0 /* free */);

	}

}
