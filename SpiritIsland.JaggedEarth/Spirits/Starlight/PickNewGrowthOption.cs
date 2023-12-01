namespace SpiritIsland.JaggedEarth;

/// <summary> Starlight's Growth </summary>
class PickNewGrowthOption {

	readonly GrowthOption[] _options;

	public PickNewGrowthOption(params GrowthOption[] options ) {
		_options = options;
	}

	public async Task ActivateAsync( Spirit self ) {
		GrowthOption option = (GrowthOption)await self.Select( "Select New Growth Option", _options, Present.Always );
		self.GrowthTrack.PickGroups.Single().Add( option ); // only works with spirits with a single pick-group
		GameState.Current.Log( new SpiritIsland.Log.LayoutChanged( $"Starlight adds Growth Option {option}" ) );
	}

}