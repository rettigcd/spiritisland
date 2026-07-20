namespace SpiritIsland.BranchAndClaw;

public class AddBagPresenceToBeastLand : SpiritAction, ISerializableSelfCmd {

	public AddBagPresenceToBeastLand():base( "Setup_PlacePresenceOnBeastLand" ) { }

	public override async Task ActAsync( Spirit self ) {
		var options = ActionScope.Current.Spaces_Existing.Where( space=>space.Beasts.Any );
		Space space = await self.SelectAlways("Add presence to",options);
		await self.Presence.Token.AddTo( space );
	}

	#region Json

	const string Tag = "AddBagPresenceToBeastLand";

	JsonArray ISerializableSelfCmd.ToJson( ISerializationContext ctx ) => new JsonArray( Tag );

	[ModuleInitializer]
	internal static void RegisterSerialization()
		=> SelfCmdRegistry.Register( Tag, ( json, ctx ) => new AddBagPresenceToBeastLand() );

	#endregion Json

}