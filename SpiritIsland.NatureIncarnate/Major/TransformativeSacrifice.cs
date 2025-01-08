namespace SpiritIsland.NatureIncarnate;

public class TransformativeSacrifice {

	public const string Name = "Transformative Sacrifice";

	[MajorCard(Name,3,"moon,fire,water,plant"),Fast]
	[AnySpirit]
	[Instructions( "Target Spirit may Remove up to 3 Presence. For each removed Presence, Take a Minor Power and play it for free. -If you have- 2 moon,3 fire,2 plant: Before taking cards, they may also Remove 1 presence from their presence track to Take a Minor Power and play it." ), Artist( Artists.KatGuevara )]
	static public async Task ActAsync(TargetSpiritCtx ctx){
		bool hasElementThreshold = await ctx.Self.Elements.YouHave("2 moon,3 fire,2 plant");
		await TargetSpiritAction( ctx.Other, hasElementThreshold );
	}

	static async Task TargetSpiritAction(Spirit spirit, bool boostFromElementThreshold ) {

		// Target Spirit may Remove up to 3 Presence.
		var tokensToDestroy = new SourceSelector(spirit.Presence.Lands)
			.UseQuota(new Quota().AddGroup(3,spirit.Presence))
			.GetEnumerator(spirit,Prompt.RemainingCount($"Destroy up to 3 to Take Minor and Play it for free."),Present.Done);
		int destroyed = 0;
		await foreach(SpaceToken? spaceToken in tokensToDestroy) {
			await spaceToken.Destroy();
			++destroyed;
		}		

		if( boostFromElementThreshold ){
			// Before taking cards,
			// they may also Remove 1 presence from their presence track to Take a Minor Power and play it.
			ITokenLocation? presenceToRemove = await spirit.Select(Prompts.SelectPresenceTo("remove from game for additional Minor."), spirit.Presence.RevealOptions(), Present.Done);

			if(presenceToRemove is not null) {
				await presenceToRemove.RemoveAsync();
				++spirit.Presence.Destroyed.Count;
				++destroyed;
			}
		}

		// Show them all at once
		DrawCardResult result = await DrawFromDeck.DrawInner(spirit,GameState.Current.MinorCards!,destroyed,destroyed);
		foreach(PowerCard? card in result.SelectedCards)
			spirit.PlayCard(card,0 /* free */);

	}

}
