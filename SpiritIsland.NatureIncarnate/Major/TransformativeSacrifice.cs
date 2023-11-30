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
		var tokensToDestroy = new SourceSelector(ctx.Self.Presence.Lands.Tokens())
			.AddGroup(3,ctx.Self.Presence)
			.GetEnumerator(ctx.Self,Prompt.RemainingCount($"Destroy up to 3 to Take Minor and Play it for free."),Present.Done);
		int destroyed = 0;
		await foreach(SpaceToken? spaceToken in tokensToDestroy) {
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
