namespace SpiritIsland.JaggedEarth;

class CleaningUpMessesIsADrag(Spirit spirit) : BaseModEntity, IHandleTokenRemoved {

	#region rule
	public const string Name = "Cleaning up Messes is a Drag";
	const string Description = "After one of your Powers Removes blight, Destroy 1 of your presence.  Ignore this rule for Let's See What Happens.";
	static public SpecialRule Rule => new SpecialRule(Name, Description);
	#endregion rule

	Task IHandleTokenRemoved.HandleTokenRemovedAsync(ITokenRemovedArgs args) {
		if(args.Removed == Token.Blight
			&& args.Reason switch { RemoveReason.Removed or RemoveReason.Replaced => true, _ => false }
			&& spirit.ActionIsMyPower
		) {
			return Cmd.DestroyPresence($"{CleaningUpMessesIsADrag.Name} Destroy presence for blight cleanup")
				.ActAsync(spirit);
		}
		return Task.CompletedTask;
	}


}