namespace SpiritIsland.NatureIncarnate;

class WoundedToken( Spirit spirit ) : SpiritPresenceToken( spirit ), IHandleTokenAdded {
	public async void HandleTokenAdded( Space to, ITokenAddedArgs args ) {
		// When your powers ...
		if(!IsYourPowers) return;

		await Serenity( to, args );
	}

	bool IsYourPowers{ get {
		ActionScope scope = ActionScope.Current;
		return scope.Category == ActionCategory.Spirit_Power && scope.Owner != Self;
	} }

	async Task Serenity(Space to, ITokenAddedArgs args ) {
		if(!EnableSerenity) return;

		// move any number of Invaders into your lands
		if(args.Added.HasTag(TokenCategory.Invader))
			// you may Downgrade 1 of those invaders (max once per Power)
			await ReplaceInvader.DowngradeSelectedInvader( to, (HumanToken)args.Added );
		// add or move any number o Dahan into one of your lands, you may Downgrade 1 Invader there (max oncer per power)
		if(args.Added.HasTag(TokenCategory.Dahan))
			// you may Downgrade 1 of those invaders (max once per Power)
			await ReplaceInvader.Downgrade1( Self, to, Present.Done, Human.Invader );
	}

	public bool EnableSerenity = false;
}