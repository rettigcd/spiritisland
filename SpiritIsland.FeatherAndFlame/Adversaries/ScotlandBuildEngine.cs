namespace SpiritIsland.FeatherAndFlame;

class ScotlandBuildEngine : BuildEngine {

	public bool ShouldChartTheCoastline { get; set; }

	public override bool ShouldBuildOnSpace( SpaceState spaceState ) 
		=> base.ShouldBuildOnSpace( spaceState ) || ChartTheCoastline( spaceState );

	bool ChartTheCoastline( SpaceState spaceState ) {
		if( !ShouldChartTheCoastline ) return false;
		bool isCharted = spaceState.Space.IsCoastal 
			&& spaceState.Adjacent.Any( adj => adj.Has( Human.City ) );
		if( isCharted )
			_chartedCoastland.Add(spaceState.Space.Text);
		return isCharted;
        
	}

	readonly HashSet<string> _chartedCoastland = new HashSet<string>();

	public override async Task ActivateCard( InvaderCard card, GameState gameState ) {
		await base.ActivateCard( card, gameState );
		if( _chartedCoastland.Count != 0) {
			ActionScope.Current.Log(new SpiritIsland.Log.Debug("Charted Coastland: by building on "+_chartedCoastland.Order().Join(",")));
			_chartedCoastland.Clear();
		}
	}

}
