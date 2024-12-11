namespace SpiritIsland.NatureIncarnate;

public class EmpoweredAbduct : IActionFactory {

	public string Title => "Abduct Explorer/Town";

	public string Text => Title;

	public bool CouldActivateDuring( Phase speed, Spirit spirit )
		=> speed == Phase.Fast && spirit.Incarna.Empowered;

	public async Task ActivateAsync( Spirit spirit ) {
		var incarna = spirit.Incarna;
		if(!incarna.IsPlaced) return;
		Space space = incarna.Space;
		HumanToken[] options = space.HumanOfAnyTag( Human.Explorer_Town );
		SpaceToken invaderToAbduct = await spirit.SelectAsync(new A.SpaceTokenDecision("Select Invader to Abduct", options.On(space), Present.Done));
		if(invaderToAbduct == null) return;

		await invaderToAbduct.MoveTo(EndlessDark.Space.ScopeSpace);
	}

}

/// <summary>
/// Adds the Empowered Abduct action when appropriate
/// </summary>
class EnableEmpoweredAbductMod : IModifyAvailableActions {
	readonly Spirit _spirit;
	public EnableEmpoweredAbductMod(Spirit spirit) {
		_spirit = spirit;
	}
	public void Modify(List<IActionFactory> orig, Phase phase) {
		if( phase == Phase.Fast && _spirit.Presence.Incarna.Empowered && !_spirit.UsedActions.Contains(_empoweredAbduct) )
			orig.Add(_empoweredAbduct);
	}
	static readonly EmpoweredAbduct _empoweredAbduct = new EmpoweredAbduct();
}

