namespace SpiritIsland.NatureIncarnate;

public class SolidifyEchoesOfMajestyPast {

	public const string Name = "Solidify Echoes of Majesty Past";

	[MajorCard(Name,4,"sun,moon,air,earth"),Fast]
	[AnySpirit]
	[Instructions( "Choose one of target Spirit's lands. In that land and each adjacent land, Defend 3. They Add 1 DestroyedPresence to each adjacent land. Skip up to 1 Invader Action at each added DestroyedPresence. -If you have- 2 sun,2 moon,2 earth: Target Spirit either Reclaims 1 Power Card or re-gains a Unique Power they previously forgot. They may play it by paying its cost." ), Artist( Artists.EmilyHancock )]
	static public async Task ActAsync(TargetSpiritCtx ctx){

		// Choose one of target Spirit's lands.
		Space center = await ctx.Self.Select(new A.Space("Select Central Hub", ctx.Other.Presence.Spaces,Present.Always));
		if(center == null) return; // is this possible?

		// In that land and each adjacent, Defend 3.
		foreach(SpaceState? space in center.Range(1).Tokens())
			space.Defend.Add(3);

		// They Add 1 DestroyedPresence to each adjacent land.
		List<Space> spacesOptions = center.Adjacent_Existing.ToList();
		while(0 < spacesOptions.Count && 0 < ctx.Other.Presence.Destroyed) {
			var space = await ctx.Other.Select(new A.Space("Place Destroyed Presence and Skip up to 1 Invader action", spacesOptions,Present.Always));
			if(space == null) break;

			await ctx.Other.Presence.PlaceDestroyedAsync(1,space);
			spacesOptions.Remove(space);

			// Skip up to 1 Invader Action at each added DestroyedPresence.
			space.Tokens.Skip1InvaderAction(Name,ctx.Self);

		}
		
		// If you have...
		if(await ctx.YouHave("2 sun,2 moon,2 earth" )) {
			PowerCard[] startingHand = ctx.Self.Hand.ToArray();
			PowerCard[] forgottenUniques = UniqueCardsForgotten(ctx.Self);
			// Target Spirit either
			await Cmd.Pick1<SelfCtx>(
				// Relaims 1 Power Card OR
				new ReclaimN(1)
					.OnlyExecuteIf(x=>0<x.Self.DiscardPile.Count),
				// re-gains a Unique Power they previously forgot.
				new SpiritAction("Re-gain a Unique Power they previously forgot", ctx=>RegainUnique(ctx.Self,forgottenUniques) )
					.OnlyExecuteIf(x=>0<forgottenUniques.Length)
			).ActAsync(ctx.OtherCtx);

			// They may play it by paying its cost.
			PowerCard? newCard = ctx.Self.Hand.Except(startingHand).SingleOrDefault();
			if(newCard != null 
				&& newCard.Cost <= ctx.Self.Energy
				&& await ctx.Self.UserSelectsFirstText($"Pay {newCard.Cost} to play {newCard.Name}?", $"Yes, pay {newCard.Cost} now.","No thank you.")
			) {
				ctx.Self.PlayCard(newCard);				
			}
		}

	}

	static async Task RegainUnique(Spirit spirit, PowerCard[] forgottenUniques) {
		var card = await spirit.SelectPowerCard( "Regain Unique",forgottenUniques,CardUse.Reclaim,Present.Always);
		if(card != null)
			spirit.Hand.Add(card);
	}

	static PowerCard[] UniqueCardsForgotten(Spirit spirit ) {
		HashSet<string> cardsIHave = spirit.InPlay.Union(spirit.Hand).Union(spirit.DiscardPile)
			.Select(x=>x.Name)
			.ToHashSet();
		Spirit s2 = (Spirit)(Activator.CreateInstance(spirit.GetType()) ?? throw new InvalidOperationException("Unable to clone spirit"));
		return s2!.Hand
			.Where(card=>!cardsIHave.Contains(card.Name))
			.ToArray();

	}

}

