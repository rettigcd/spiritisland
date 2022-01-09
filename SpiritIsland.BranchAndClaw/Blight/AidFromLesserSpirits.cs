namespace SpiritIsland.BranchAndClaw {

	class AidFromLesserSpirits : BlightCardBase {

		public AidFromLesserSpirits():base("Aid from Lesser Spirits", 2 ) { }

		public override ActionOption<GameState> Immediately => 

			new ActionOption<GameState>("Distribute N+1 cards to players that can play for free each round.", async gs => {
				// Draw 1 minor power card per player plus 1 more.
				var cards = gs.MinorCards.Flip(gs.Spirits.Length+1);
				foreach(var spirit in gs.Spirits ){

					// Give 1 to each spirit.
					var card = await spirit.SelectPowerCard("Pick card to play every turn for free.", cards, CardUse.AddToHand, Present.Always);
					cards.Remove(card);

					// They may be used every turn as if played, but cost no card plays/energy.
					// (don't put card in hand, because we don't want them discarding or forgetting, or playing normally.)
					// (Just add it and its elements every time.)
					spirit.EnergyCollected += ( Spirit s ) => {
						s.AddActionFactory( card );
						s.Elements.AddRange( card.Elements );
					};

				}

				// Place unselected cards into the minor powers discard pile.
				gs.MinorCards.Discard(cards);

			} );

	}

}
