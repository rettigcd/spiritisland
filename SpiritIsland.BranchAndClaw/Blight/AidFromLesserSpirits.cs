namespace SpiritIsland.BranchAndClaw;

public class AidFromLesserSpirits : BlightCard {

	public AidFromLesserSpirits():base("Aid from Lesser Spirits", "Immediatly, draw 1 Minor Power Card per player plus 1 more.  Give 1 to each Spirit.  They may be used every turn as if played, but cost no Card Plays/Energy.  Place unselected card in Minor Powers discard pile.", 2 ) { }

	public override DecisionOption<GameCtx> Immediately => 

		new DecisionOption<GameCtx>("Distribute N+1 cards to players that can play for free each round.", async ctx => {
			var gs = ctx.GameState;
			// Draw 1 minor power card per player plus 1 more.
			var cards = gs.MinorCards.Flip(gs.Spirits.Length+1);
			foreach(var spirit in gs.Spirits ){

				// Give 1 to each spirit.
				var card = await spirit.SelectPowerCard("Pick card to play every turn for free.", cards, CardUse.AddToHand, Present.Always);
				cards.Remove(card);

				// They may be used every turn as if played, but cost no card plays/energy.
				// (don't put card in hand, because we don't want them discarding or forgetting, or playing normally.)
				// (Just add it and its elements every time.)
				spirit.EnergyCollected.Add( ( Spirit s ) => {
					s.AddActionFactory( card );
					s.Elements.AddRange( card.Elements );
				});

			}

			// Place unselected cards into the minor powers discard pile.
			gs.MinorCards.Discard(cards);

		} );

}