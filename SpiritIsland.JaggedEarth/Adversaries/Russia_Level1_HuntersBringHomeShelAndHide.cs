namespace SpiritIsland.JaggedEarth;

/// <summary>
/// Tracks Beast destroyed by adding ravage Blight
/// </summary>
class Russia_Level1_HuntersBringHomeShelAndHide : BaseModEntity, IHandleTokenAddedAsync {

	#region Loss Condition

	public static AdversaryLossCondition HuntersSwarmTheIsland => new AdversaryLossCondition(
		"Hunters Swarm the Island: Put Beasts Destroyed by Adversary rules on this panel. If there are ever more Beasts on this panel than on the island, the Invaders win.",
		HuntersSwarmTheIslandImp
	);

	static void HuntersSwarmTheIslandImp( GameState gameState ) {

		int destroyedCardBeasts = AdversaryCard.ScopeTokens.Beasts.Count;

		// Put beast Destroyed by Adversary rules on this panel.If there are ever more beast on this panel than on the island, the Invaders win.
		int remainingBeasts = ActionScope.Current.Tokens_Unfiltered.Sum( s => s.Beasts.Count );
		if(remainingBeasts < destroyedCardBeasts)
			GameOverException.Lost( $"Russia-Hunters Swarm the Island (beasts remaining:{remainingBeasts} killed:{destroyedCardBeasts})" );
	}

	#endregion

	async Task IHandleTokenAddedAsync.HandleTokenAddedAsync( SpaceState to, ITokenAddedArgs args ) {
		if(args.Added == Token.Blight
			&& args.Reason == AddReason.Ravage
		) {
			var beasts = to.Beasts;
			if(beasts.Any) {
				await beasts.Destroy( 1 );
				AdversaryCard.ScopeTokens.Adjust( args.Added, 1 );
				ActionScope.Current.LogDebug( $"Blight on {((Space)args.To).Text} destroys 1 beast." );
			}
		}
	}

	static readonly public FakeSpace AdversaryCard = new FakeSpace( "Russia Adv Card" );
}
