namespace SpiritIsland.NatureIncarnate;

class SenselessRoaming(Spirit spirit) : BaseModEntity, IHandleTokenAdded {

	#region Name/Rule
	public const string Name = "Senseless Roaming";
	const string Description = "When your Actions add Strife to an Explorer/Town, you may Push it.";
	static public SpecialRule Rule => new SpecialRule( Name, Description );
	#endregion Name/Rule

	Task IHandleTokenAdded.HandleTokenAddedAsync(Space to, ITokenAddedArgs args) {
		return args.IsStrifeAdded() && args.Added.HasAny(Human.Explorer_Town) && args.To is Space space && spirit.ActionIsMyPower
			? args.Added.On(space).PushAsync(spirit)
			: Task.CompletedTask;
	}

}