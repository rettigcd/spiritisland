namespace SpiritIsland.NatureIncarnate;

public class SolidifyEchoesOfMajestyPast {

	public const string Name = "Solidify Echoes of Majesty Past";

	[MajorCard(Name,4,"sun,moon,air,earth"),Fast]
	[AnySpirit]
	[Instructions( "Choose one of target Spirit's lands. In that land and each adjacent land, Defend 3. They Add 1 DestroyedPresence to each adjacent land. Skip up to 1 Invader Action at each added DestroyedPresence. -If you have- 2 sun,2 moon,2 earth: Target Spirit either Reclaims 1 Power Card or re-gains a Unique Power they previously forgot. They may play it by paying its cost." ), Artist( Artists.EmilyHancock )]
	static public async Task ActAsync(TargetSpiritCtx ctx){

		// Choose one of target Spirit's lands.
		Space? center = await ctx.Self.Select("Select Central Hub", ctx.Other.Presence.Lands,Present.Always);
		if(center is null) return; // is this possible?

		// In that land and each adjacent, Defend 3.
		foreach(Space? space in center.Range(1))
			space.Defend.Add(3);

		// They Add 1 DestroyedPresence to each adjacent land.
		List<Space> spacesOptions = center.Adjacent_Existing.ToList();
		while(0 < spacesOptions.Count && 0 < ctx.Other.Presence.Destroyed.Count) {
			Space? space = await ctx.Other.Select("Place Destroyed Presence and Skip up to 1 Invader action", spacesOptions,Present.Always);
			if(space is null) break;

			await ctx.Other.Presence.Destroyed.MoveToAsync(space);
			spacesOptions.Remove(space);

			// Skip up to 1 Invader Action at each added DestroyedPresence.
			space.Skip1InvaderAction(Name,ctx.Self);

		}
		
		// If you have...
		if(await ctx.YouHave("2 sun,2 moon,2 earth" )) {
			PowerCard[] startingHand = [.. ctx.Self.Hand];
			PowerCard[] forgottenUniques = UniqueCardsForgotten(ctx.Self);
			// Target Spirit either
			await Cmd.Pick1(
				// Relaims 1 Power Card OR
				new ReclaimN(1)
					.OnlyExecuteIf(self=>0<self.DiscardPile.Count),
				// re-gains a Unique Power they previously forgot.
				new SpiritAction("Re-gain a Unique Power they previously forgot", self=>RegainUnique(self,forgottenUniques) )
					.OnlyExecuteIf(x=>0<forgottenUniques.Length)
			).ActAsync(ctx.Other);

			// They may play it by paying its cost.
			PowerCard? newCard = ctx.Self.Hand.Except(startingHand).SingleOrDefault();
			if(newCard != null 
				&& newCard.Cost <= ctx.Self.Energy
				&& await ctx.Self.UserSelectsFirstText($"Pay {newCard.Cost} to play {newCard.Title}?", $"Yes, pay {newCard.Cost} now.","No thank you.")
			) {
				ctx.Self.PlayCard(newCard);				
			}
		}

	}

	static async Task RegainUnique(Spirit spirit, PowerCard[] forgottenUniques) {
		var card = await spirit.SelectPowerCard( "Regain Unique",1,forgottenUniques,CardUse.Reclaim,Present.Always);
		if(card != null)
			spirit.Hand.Add(card);
	}

	static PowerCard[] UniqueCardsForgotten(Spirit spirit ) {
		HashSet<string> cardsIHave = spirit.InPlay.Union(spirit.Hand).Union(spirit.DiscardPile)
			.Select(x=>x.Title)
			.ToHashSet();
		Spirit s2 = (Spirit)(Activator.CreateInstance(spirit.GetType()) ?? throw new InvalidOperationException("Unable to clone spirit"));
		return s2!.Hand
			.Where(card=>!cardsIHave.Contains(card.Title))
			.ToArray();

	}

}

