namespace SpiritIsland.NatureIncarnate;

public class TerrorStalksTheLand : SpaceState {

	static public SpecialRule Rule => new SpecialRule( "Terror Stalks the Land", "You have an Incarna. You may Abduct 1 Explorer / Town at empowered Incarna each Fast Phase. To Abduct a piece, Move it to tThe Endless Dark.  When pieces Escape, Move them to a non-Ocean land with your Presence/Incarna.  If they have no legal land to move to, you lose.  When your Powers would directly damage or directly destroy the only Invader in a land, instead Abduct it." );

	public TerrorStalksTheLand( SpaceState spaceState )
		: base( spaceState ) {
	}

	/// <remarks>Destroys using SpaceState.Remove()</remarks>
	public override async Task DestroySpace() {
		// Destroy Invaders
		await Invaders.DestroyAll( Human.Invader ); // eventually comes back to .Remove(...)
	}

	/// <remarks>Destroys invaders using .DestroyNInvaders()</remarks>
	public override async Task<TokenRemovedArgs> Remove( IToken token, int count, RemoveReason reason = RemoveReason.Removed ) {
		// not destroying invaders - do normal stuff
		if(reason != RemoveReason.Destroyed || token.Class.Category != TokenCategory.Invader)
			return await base.Remove( token, count, reason );

		// Destroying Invaders
		int destroyed = await DestroyNInvaders( token.AsHuman(), count );
		return new TokenRemovedArgs( token, reason, this, destroyed );
	}

	/// <remarks>Checks # of invaders in land 1st time it is called.</remarks>
	/// This => Base.DestroyNInvaders => Token.Destory => Tokens.Remove => This
	public override async Task<int> DestroyNInvaders( HumanToken invaderToDestroy, int countToDestroy ) {
		countToDestroy = Math.Min( countToDestroy, this[invaderToDestroy] );
		if(countToDestroy == 0) throw new InvalidOperationException( "Can't destroy invaders because they aren't here." );

		if(!PreviouslyDestroyed && InvaderCount == 1) {
			// abduct it
			await AbductInvader( invaderToDestroy );
			return 0;
		} else {
			// track that we are destroying something
			ActionScope.Current[destroyedAnInvaderKey] = true;
			return await base.DestroyNInvaders( invaderToDestroy, countToDestroy );
		}

	}

	async Task AbductInvader( HumanToken invaderToDestroy ) {
		var spaceTokenToRemove = new SpaceToken( Space, invaderToDestroy );

		var invaderToAddToEndlessDark = ActionScope.Current.SafeGet<HumanToken>( damagedAnInvaderKey, invaderToDestroy );

		if(invaderToAddToEndlessDark == invaderToDestroy)
			await spaceTokenToRemove.MoveTo( EndlessDark.Space );
		else {
			await spaceTokenToRemove.Remove();
			await EndlessDark.Space.Tokens.Add( invaderToAddToEndlessDark, 1 );
		}
	}

	static bool PreviouslyDestroyed 
		=> ActionScope.Current.SafeGet<bool>( destroyedAnInvaderKey, false );

	const string destroyedAnInvaderKey = "DestroyedAnInvader";
	const string damagedAnInvaderKey = "DamagedAnInvader";
	int InvaderCount => this.SumAny( Human.Invader );

	public override InvaderBinding Invaders {
		get {
			var invader = base.Invaders;
			invader.InvaderDamaged += Invader_InvaderDamaged;
			return invader;
		}
	}

	void Invader_InvaderDamaged( HumanToken orig ) {
		var scope = ActionScope.Current;
		if(!PreviouslyDestroyed && InvaderCount == 1 && !scope.ContainsKey(damagedAnInvaderKey))
			scope[damagedAnInvaderKey] = orig;
	}
}