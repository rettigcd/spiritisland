namespace SpiritIsland.Basegame;

/// <summary> Adds a new presence (from the bag) to a Range-1 Coast. </summary>
/// <remarks> Ocean set up. </remarks>
public class AddBagPresenceToCostal : SpiritAction {

	public AddBagPresenceToCostal():base( "Place Presence in Costal" ) { }

	// ! Can't use normal PlacePresence, because it must be range-1, range 0 not allowed.
	public override async Task ActAsync( Spirit self ) {
		IEnumerable<Space> options = self.Presence.Lands.First().Adjacent_Existing;
		Space space = await self.SelectAlways("Add presence to", options);
		await self.Presence.Token.AddTo( space );
	}

}