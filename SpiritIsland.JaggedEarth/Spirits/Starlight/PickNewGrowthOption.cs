namespace SpiritIsland.JaggedEarth;

/// <summary> Starlight's Growth </summary>
class PickNewGrowthOption( params GrowthGroup[] options ) {

	readonly GrowthGroup[] _options = options;

	public async Task ActivateAsync( Spirit self ) {
		GrowthGroup option = await self.SelectAlways( "Select New Growth Option", _options );
		self.GrowthTrack.PickGroups.Single().Add( option ); // only works with spirits with a single pick-group
		ActionScope.Current.Log( new SpiritIsland.Log.LayoutChanged( $"Starlight adds Growth Option {option}" ) );
	}

}