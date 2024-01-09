namespace SpiritIsland.Basegame;

//	Level 1 - Heavy Mining: >=6 +1 blight
//	The additional Blight does not destroy Presence or cause cascades.
class SwedenHeavyMining : BaseModEntity, IHandleTokenAddedAsync, IReactToLandDamage {

	public bool MiningRush { get; set; }

	async Task IReactToLandDamage.HandleDamageAddedAsync( SpaceState tokens, int _ ) {
		//	Level 1 - Heavy Mining: >=6 +1 blight
		//	The additional Blight does not destroy Presence or cause cascades.
		if(6 <= tokens[LandDamage.Token]) {

			var config = BlightToken.ForThisAction;
			config.DestroyPresence = false;
			config.ShouldCascade = false;

			await tokens.Blight.AddAsync( 1 );
			ActionScope.Current.LogDebug( "Heavy Mining: additional blight on " + tokens.Space.Text );
		}
	}

	public async Task HandleTokenAddedAsync(SpaceState to, ITokenAddedArgs args ) {

		// Level 5 - Mining Rush: blight => +1 town on adjacent land 
		if(MiningRush)
			// When ravage adds at least 1 blight to a land
			if(args.Reason == AddReason.Ravage && args.Added == Token.Blight) {
				var noBuildAdjacents = to.Adjacent
					.Where( adj => !adj.HasAny( Human.Town_City ) )
					.ToArray();

				var spirit = to.Space.Boards[0].FindSpirit();

				Space selection = await spirit.SelectAsync( A.Space.ToPlaceToken( 
					"Mining Rush: Place Town", 
					noBuildAdjacents.Downgrade(), 
					Present.Always, 
					to.GetDefault( Human.Town ) 
				) );
				if(selection != null) {
					selection.Tokens.AdjustDefault( Human.Town, 1 );
					ActionScope.Current.LogDebug( $"Mining Rush: Blight on {((Space)args.To).Text} caused +1 Town on {selection.Text}." );
				}
			}

	}
}
