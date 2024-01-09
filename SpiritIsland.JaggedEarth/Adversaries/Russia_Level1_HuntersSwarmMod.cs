namespace SpiritIsland.JaggedEarth;

/// <summary>
/// Tracks Beast destroyed by adding ravage Blight
/// </summary>
class Russia_Level1_HuntersSwarmMod : BaseModEntity, IHandleTokenAddedAsync {

	public void HuntersSwarmTheIsland( GameState gameState ) {
		// Put beast Destroyed by Adversary rules on this panel.If there are ever more beast on this panel than on the island, the Invaders win.
		int remainingBeasts = gameState.Spaces_Unfiltered.Sum( s => s.Beasts.Count );
		if(remainingBeasts < _beastsDestroyed)
			GameOverException.Lost( $"Russia-Hunters Swarm the Island (beasts remaining:{remainingBeasts} killed:{_beastsDestroyed})" );
	}

	async Task IHandleTokenAddedAsync.HandleTokenAddedAsync( SpaceState to, ITokenAddedArgs args ) {
		if(args.Added == Token.Blight
			&& args.Reason == AddReason.Ravage
		) {
			var beasts = to.Beasts;
			if(beasts.Any) {
				await beasts.Destroy( 1 );
				_beastsDestroyed++;
				ActionScope.Current.LogDebug( $"Blight on {((Space)args.To).Text} destroys 1 beast." );
			}
		}
	}
	int _beastsDestroyed = 0;


}
