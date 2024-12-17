namespace SpiritIsland.NatureIncarnate;

public class FavorsOfStoryAndSeason {

	public const string Name = "Favors of Story and Season";

	[SpiritCard(FavorsOfStoryAndSeason.Name, 1, Element.Sun,Element.Earth,Element.Plant,Element.Animal),Fast,AnotherSpirit]
	[Instructions("Target Spirit may Gather up to 3 Dahan into one of their lands. If they have at least 3 Dahan among their lands, they gain 1 Energy and may Reclaim 1 Power Card instead of discarding it at the end of turn."), Artist(Artists.AalaaYassin)]
	static public async Task ActAsync( TargetSpiritCtx ctx ){
		// Target Spirit may Gather up to 3 Dahan into one of their lands.
		await Cmd.GatherUpToNDahan( 3 )
			.To().SpiritPickedLand().Which( Has.YourPresence )
			.ActAsync(ctx.Other);

		// If they have at least 3 Dahan among their lands,
		if(3 <= ctx.Other.Presence.Lands.Sum( t => t.Dahan.CountAll )) {
			// they gain 1 Energy
			++ctx.Other.Energy;
			// and may Reclaim 1 Power Card instead of discarding it at the end of turn.
			await Cmd.Reclaim1CardInsteadOfDiscarding.ActAsync( ctx.Self );
		}

	}

}
