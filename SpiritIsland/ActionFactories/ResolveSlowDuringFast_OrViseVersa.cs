namespace SpiritIsland;

/// <summary>
/// Action added to a spirits Resolvable-Action list that allows them to resolve an out-of-phase card during Slow or Fast.
/// </summary>
public class ResolveSlowDuringFast_OrViseVersa : IActionFactory, ISerializableActionFactory {

	public bool CouldActivateDuring( Phase phase, Spirit _ ) => phase == Phase.Fast || phase == Phase.Slow;

	string IOption.Text => Title;

	public string Title => "Resolve Other Speed Action";

	public Task ActivateAsync(Spirit self) => ResolveOutOfPhaseAction.Execute( self );

	#region Json

	public JsonArray ToJson( ISerializationContext ctx ) => new JsonArray( "ResolveSlowDuringFast_OrViseVersa" );

	[ModuleInitializer]
	internal static void RegisterSerialization()
		=> ActionFactoryRegistry.Register( "ResolveSlowDuringFast_OrViseVersa", ( json, ctx ) => new ResolveSlowDuringFast_OrViseVersa() );

	#endregion Json

}

/// <summary>
/// Action added to a spirits Resolvable-Action list that allows them to resolve an Slow card during Fast
/// </summary>
public class ResolveSlowDuringFast : IActionFactory, ISerializableActionFactory {

	public bool CouldActivateDuring( Phase phase, Spirit _ ) => phase == Phase.Fast;

	string IOption.Text => Title;

	public string Title => "Resolve Slow Action";

	public Task ActivateAsync( Spirit self ) => ResolveOutOfPhaseAction.Execute( self );

	#region Json

	public JsonArray ToJson( ISerializationContext ctx ) => new JsonArray( "ResolveSlowDuringFast" );

	[ModuleInitializer]
	internal static void RegisterSerialization()
		=> ActionFactoryRegistry.Register( "ResolveSlowDuringFast", ( json, ctx ) => new ResolveSlowDuringFast() );

	#endregion Json

}

