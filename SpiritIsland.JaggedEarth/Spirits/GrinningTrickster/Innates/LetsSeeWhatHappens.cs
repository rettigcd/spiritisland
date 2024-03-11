namespace SpiritIsland.JaggedEarth;

[InnatePower("Let's See What Happens"), Fast, FromPresence(1,Filter.Invaders)]
public class LetsSeeWhatHappens {

	[InnateTier("1 moon,1 fire,2 air","Discard Minor Powers from the deck until you get one that targets a land. Use immediately. All 'up to' instructions must be used at max and ORs treated as ANDs ")]
	static public async Task Option1(TargetSpaceCtx ctx ) {
		await DawCardAndActivateAtMax( ctx );
	}

	[InnateTier("2 moon,1 fire,2 air","You may Forget a Power Card to gain the just-used Power Card and 1 Energy")]
	static public async Task Option2(TargetSpaceCtx ctx ) {

		PowerCard card = await DawCardAndActivateAtMax( ctx );

		// You MAY Forget a Power Card to gain the just-used Power Card and 1 energy.
		if(! await ctx.Self.UserSelectsFirstText( "Keep Card by forgetting another card?", "Yes, keep " + card.Title, "No, thank you." )) return;

		ctx.Self.AddCardToHand( card );
		await ctx.Self.ForgetACard();

		if(ctx.Self.Hand.Contains( card ))
			ctx.Self.Energy++;
	}

	static async Task<PowerCard> DawCardAndActivateAtMax( TargetSpaceCtx ctx ) {

		// Discard Minor Powers from the deck until you get one that targets a land.
		PowerCard card = DiscardMinorPowersUntilYouTargetLand();

		// Show to the user what card is being triggered
		await ctx.Self.SelectFactory( "Perform All Action at Max", [ card ] );

		// Use immediately. All 'up to' instructions must be used at max and 'OR's treated as 'AND's "
		await using var maxScope = await ActionScope.Start(ActionCategory.Spirit_Power);
		maxScope.Owner = ctx.Self;
		maxScope.Upgrader = (x) => new TricksterTokens(ctx.Self,x,runAtMax:true);

		await card.InvokeOn( new LetsSeeWhatHappensCtx( ctx ) );
		return card;
	}

	static PowerCard DiscardMinorPowersUntilYouTargetLand() {
		PowerCard card;
		do { 
			card = GameState.Current.MinorCards.FlipNext(); 
		} while(card.LandOrSpirit != LandOrSpirit.Land);
		return card;
	}

}