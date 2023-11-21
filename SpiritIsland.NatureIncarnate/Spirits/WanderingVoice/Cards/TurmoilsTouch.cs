namespace SpiritIsland.NatureIncarnate;

public class TurmoilsTouch {

	public const string Name = "Turmoil's Touch";

	[SpiritCard(Name, 0, Element.Sun,Element.Moon,Element.Air,Element.Plant)]
	[Slow, AnotherSpirit]
	[Instructions( "Target Spirit may either pay 1 Enery or dicard a Power Card (from hand) to Take a Minor Power into their discard. You may do likewise." ), Artist( Artists.EmilyHancock )]
	static public async Task ActionAsync(TargetSpiritCtx ctx) {
		await PayAndTakeCard( ctx.OtherCtx);
		await PayAndTakeCard( ctx );
	}

	static Task PayAndTakeCard( SelfCtx ctx ) {
		return Cmd.Pick1(
			new SpiritAction(
				"Pay 1 Energy to Take a Minor Power into their discard.", 
				async ctx => { --ctx.Self.Energy; await TakeMinorIntoDiscard(ctx); }
			).OnlyExecuteIf(x=>0<x.Self.Energy),
			new SpiritAction(
				"Discard a Power Card to Take a Minor Power into their discard.", 
				async ctx => { 
					await new DiscardCard("Discard 1 card from hand", s=>s.Hand).ActAsync( ctx ); 
					await TakeMinorIntoDiscard(ctx); }
			),
			new SpiritAction("No, thank you.",_=>Task.CompletedTask)
		).ActAsync( ctx );
	}

	static async Task TakeMinorIntoDiscard( SelfCtx ctx ) {
		// Draw into HAND
		var card = (await DrawFromDeck.DrawInner(ctx.Self,GameState.Current.MinorCards, 1, 1)).Selected;
		// Move from HAND to Discard
		ctx.Self.Hand.Remove( card );
		ctx.Self.DiscardPile.Add( card );
	}

}