namespace SpiritIsland.Basegame;

/// <summary> Adds a new presence (from the bag) to a Range-1 Coast. </summary>
/// <remarks> Ocean set up. </remarks>
public class AddBagPresenceToCostal : SpiritAction, ISerializableSelfCmd {

	public AddBagPresenceToCostal():base( "Place Presence in Costal" ) { }

	// ! Can't use normal PlacePresence, because it must be range-1, range 0 not allowed.
	public override async Task ActAsync( Spirit self ) {
		IEnumerable<Space> options = self.Presence.Lands.First().Adjacent_Existing;
		Space space = await self.SelectAlways("Add presence to", options);
		await self.Presence.Token.AddTo( space );
	}

	#region Json

	const string Tag = "AddBagPresenceToCostal";

	JsonArray ISerializableSelfCmd.ToJson( ISerializationContext ctx ) => new JsonArray( Tag );

	[ModuleInitializer]
	internal static void RegisterSerialization()
		=> SelfCmdRegistry.Register( Tag, ( json, ctx ) => new AddBagPresenceToCostal() );

	#endregion Json

}