namespace SpiritIsland;

/// <summary>
/// Action added to a spirits Resolvable-Action list that allows them to resolve an out-of-phase card during Slow or Fast.
/// </summary>
public class ResolveSlowDuringFast_OrViseVersa : IActionFactory {

	public bool CouldActivateDuring( Phase phase, Spirit _ ) => phase == Phase.Fast || phase == Phase.Slow;

	public string Name => "Resolve Other Speed Action";

	public string Text => Name;

	public Task ActivateAsync(SelfCtx ctx) => ResolveOutOfPhaseAction.Execute( ctx );

}

/// <summary>
/// Action added to a spirits Resolvable-Action list that allows them to resolve an Slow card during Fast
/// </summary>
public class ResolveSlowDuringFast : IActionFactory {

	public bool CouldActivateDuring( Phase phase, Spirit _ ) => phase == Phase.Fast;

	public string Name => "Resolve Slow Action";

	public string Text => Name;

	public Task ActivateAsync( SelfCtx ctx ) => ResolveOutOfPhaseAction.Execute( ctx );

}

