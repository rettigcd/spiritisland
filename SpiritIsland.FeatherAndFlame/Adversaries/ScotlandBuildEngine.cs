namespace SpiritIsland.FeatherAndFlame;

class ScotlandBuildEngine : BuildEngine {

	public override bool ShouldBuildOnSpace( Space space ) 
		=> base.ShouldBuildOnSpace( space ) || ChartTheCoastline( space );

	bool ChartTheCoastline( Space space ) {

		// Make sure space is not isolated.
		bool isCharted = space.SpaceSpec.IsCoastal && space.IsConnected
			&& space.Adjacent.Any( adj => adj.Has( Human.City ) && adj.IsConnected );

		if( isCharted )
			_chartedCoastland.Add(space.Label);
		return isCharted;
	}

	readonly HashSet<string> _chartedCoastland = [];

	public override async Task ActivateCard( InvaderCard card, GameState gameState ) {
		await base.ActivateCard( card, gameState );
		if( _chartedCoastland.Count != 0) {
			ActionScope.Current.Log(new SpiritIsland.Log.Debug("Charted Coastland: by building on "+_chartedCoastland.Order().Join(",")));
			_chartedCoastland.Clear();
		}
	}

}
