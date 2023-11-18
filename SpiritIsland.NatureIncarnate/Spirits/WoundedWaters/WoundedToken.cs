namespace SpiritIsland.NatureIncarnate;

class WoundedToken : SpiritPresenceToken, IHandleTokenAdded {

	public WoundedToken( Spirit spirit ) : base( spirit ) {}

	public async void HandleTokenAdded( ITokenAddedArgs args ) {
		// When your powers ...
		if(!IsYourPowers) return;

		await Serenity( args );
	}

	bool IsYourPowers{ get {
		ActionScope scope = ActionScope.Current;
		return scope.Category == ActionCategory.Spirit_Power && scope.Owner != Self;
	} }

	async Task Serenity( ITokenAddedArgs args ) {
		if(!EnableSerenity) return;

		// move any number of Invaders into your lands
		if(args.Added.HasTag(TokenCategory.Invader))
			// you may Downgrade 1 of those invaders (max once per Power)
			await ReplaceInvader.DowngradeSelectedInvader( args.To, (HumanToken)args.Added );
		// add or move any number o Dahan into one of your lands, you may Downgrade 1 Invader there (max oncer per power)
		if(args.Added.HasTag(TokenCategory.Dahan))
			// you may Downgrade 1 of those invaders (max once per Power)
			await ReplaceInvader.Downgrade1( Self, args.To, Present.Done, Human.Invader );
	}

	public bool EnableSerenity = false;
}