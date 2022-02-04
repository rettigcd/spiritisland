namespace SpiritIsland.JaggedEarth;

[InnatePower("Let's See What Happens"), Fast, FromPresence(1,Target.Invaders)]
public class LetsSeeWhatHappens {

	[InnateOption("1 moon,1 fire,2 air","Discard Minor Powers from the deck until you get one that targets a land. Use immediately. All 'up to' instructions must be used at max and ORs treated as ANDs ")]
	static public async Task Option1(TargetSpaceCtx ctx ) {
		// Discard Minor Powers from the deck until you get one that targets a land.
		PowerCard card = DiscardMinorPowersUntilYouTargetLand( ctx );

		// Show to the user what card is being triggered
		await ctx.Self.SelectFactory("Perform All Action at Max", new IActionFactory[]{ card } );

		// Use immediately. All 'up to' instructions must be used at max and 'OR's treated as 'AND's "
		await card.InvokeOn( new LetsSeeWhatHappensCtx( ctx ) );

	}

	[InnateOption("2 moon,1 fire,2 air","You may Forget a Power Card to gain the just-used Power Card and 1 Energy")]
	static public async Task Option2(TargetSpaceCtx ctx ) {
		// Discard Minor Powers from the deck until you get one that targets a land.
		PowerCard card = DiscardMinorPowersUntilYouTargetLand( ctx );

		// Show to the user what card is being triggered
		await ctx.Self.SelectFactory("Perform All Action at Max", new IActionFactory[]{ card } );

		// Use immediately. All 'up to' instructions must be used at max and 'OR's treated as 'AND's "
		await card.InvokeOn( new LetsSeeWhatHappensCtx( ctx ) );

		// You may Forget a Power Card to gain the just-used Power Card and 1 energy.
		ctx.Self.AddCardToHand(card);
		await ctx.Self.ForgetPowerCard_UserChoice();

		if(ctx.Self.Hand.Contains(card))
			ctx.Self.Energy++;
	}

	static PowerCard DiscardMinorPowersUntilYouTargetLand( TargetSpaceCtx ctx ) {
		PowerCard card;
		do { card = ctx.GameState.MinorCards.FlipNext(); } while(card.LandOrSpirit != LandOrSpirit.Land);
		return card;
	}

}