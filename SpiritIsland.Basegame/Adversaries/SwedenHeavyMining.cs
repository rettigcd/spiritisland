namespace SpiritIsland.Basegame;

//	Level 1 - Heavy Mining: >=6 +1 blight
//	The additional Blight does not destroy Presence or cause cascades.
class SwedenHeavyMining : BaseModEntity, IHandleTokenAddedAsync, IReactToLandDamage {

	public bool MiningRush { get; set; }

	async Task IReactToLandDamage.HandleDamageAddedAsync( Space space, int _ ) {
		//	Level 1 - Heavy Mining: >=6 +1 blight
		//	The additional Blight does not destroy Presence or cause cascades.
		if(6 <= space[LandDamage.Token]) {

			var config = BlightToken.ScopeConfig;
			config.DestroyPresence = false;
			config.ShouldCascade = false;

			await space.Blight.AddAsync( 1 );
			ActionScope.Current.LogDebug( "Heavy Mining: additional blight on " + space.Label );
		}
	}

	public async Task HandleTokenAddedAsync(Space to, ITokenAddedArgs args ) {

		// Level 5 - Mining Rush: blight => +1 town on adjacent land 
		if(MiningRush)
			// When ravage adds at least 1 blight to a land
			if(args.Reason == AddReason.Ravage && args.Added == Token.Blight) {
				var noBuildAdjacents = to.Adjacent
					.Where( adj => !adj.HasAny( Human.Town_City ) )
					.ToArray();

				var spirit = to.SpaceSpec.Boards[0].FindSpirit();

				Space selection = await spirit.SelectAsync( A.SpaceDecision.ToPlaceToken( 
					"Mining Rush: Place Town", 
					noBuildAdjacents,
					Present.Always, 
					to.GetDefault( Human.Town ) 
				) );
				if(selection != null) {
					await selection.AddDefaultAsync( Human.Town, 1 );
					ActionScope.Current.LogDebug( $"Mining Rush: Blight on {((IOption)args.To).Text} caused +1 Town on {selection.Label}." );
				}
			}

	}
}
