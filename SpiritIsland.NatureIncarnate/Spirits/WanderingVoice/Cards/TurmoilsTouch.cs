namespace SpiritIsland.NatureIncarnate;

public class TurmoilsTouch {

	public const string Name = "Turmoil's Touch";

	[SpiritCard(Name, 0, Element.Sun,Element.Moon,Element.Air,Element.Plant)]
	[Slow, AnotherSpirit]
	[Instructions( "Target Spirit may either pay 1 Enery or dicard a Power Card (from hand) to Take a Minor Power into their discard. You may do likewise." ), Artist( Artists.EmilyHancock )]
	static public async Task ActAsync(TargetSpiritCtx ctx) {
		await PayAndTakeCard( ctx.Other );
		await PayAndTakeCard( ctx.Self );
	}

	static Task PayAndTakeCard( Spirit self ) {
		return Cmd.Pick1(
			new SpiritAction(
				"Pay 1 Energy to Take a Minor Power into their discard.", 
				async self => {
					--self.Energy; 
					await TakeMinorIntoDiscard(self);
				}
			).OnlyExecuteIf(spirit=>0<spirit.Energy),
			new SpiritAction(
				"Discard a Power Card to Take a Minor Power into their discard.", 
				async ctx => { 
					await new DiscardCards("Discard 1 card from hand", s=>s.Hand).ActAsync( self ); 
					await TakeMinorIntoDiscard(ctx);
				}
			),
			SpiritAction.NoAction
		).ActAsync( self );
	}

	static async Task TakeMinorIntoDiscard( Spirit spirit ) {
		// Draw into HAND
		var card = (await DrawFromDeck.DrawInner(spirit,GameState.Current.MinorCards, 1, 1)).Selected;
		// Move from HAND to Discard
		spirit.Hand.Remove( card );
		spirit.DiscardPile.Add( card );
	}

}