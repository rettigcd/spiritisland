namespace SpiritIsland.NatureIncarnate;

/// <summary>
/// Changes Space so that if a single Invader were destroyed, instead it gets Abducted.
/// </summary>
public class TerrorStalksTheLand( Space space ) : Space( space ) {

	static public SpecialRule Rule => new SpecialRule( "Terror Stalks the Land", "You have an Incarna. You may Abduct 1 Explorer / Town at empowered Incarna each Fast Phase. To Abduct a piece, Move it to the Endless Dark.  When pieces Escape, Move them to a non-Ocean land with your Presence/Incarna.  If they have no legal land to move to, you lose.  When your Powers would directly damage or directly destroy the only Invader in a land, instead Abduct it." );

	/// <remarks>Destroys using Space.Remove()</remarks>
	public override async Task DestroySpace() {
		// Destroy Invaders
		await Invaders.DestroyAll( Human.Invader ); // eventually comes back to .Remove(...)
	}

	public async override Task<int> UserSelected_DamageInvadersAsync(Spirit damagePicker, int damage, params ITokenClass[] allowedTypes) {
		bool shouldAbduct = InvaderCount == 1
			&& !PreviouslyDestroyed;

		if( shouldAbduct ) {
			await AbductInvader( InvaderTokens().First() );
			return 0;
		}

		return await base.UserSelected_DamageInvadersAsync(damagePicker, damage, allowedTypes);
	}

	public override async Task<(ITokenRemovedArgs, Func<ITokenRemovedArgs, Task>)> 
	SourceAsync( IToken token, int count, RemoveReason reason = RemoveReason.Removed ) {
		if(reason == DestroyingFromDamage.TriggerReason)
			reason = RemoveReason.Destroyed;

		bool destroyingTheOnlyInvader = reason == RemoveReason.Destroyed
			&& token.HasTag(TokenCategory.Invader)
			&& !PreviouslyDestroyed
			&& InvaderCount == 1;

		if( destroyingTheOnlyInvader ) {
			RemovingTokenCtx removedHandlers = RemovedHandlerSnapshop;
			await AbductInvader(token.AsHuman());
			return (
				new TokenRemovedArgs(this, token, 1, RemoveReason.Abducted),
				removedHandlers.NotifyRemoved
			);
		}

		// Just do the normal thing.
		return await base.SourceAsync(token, count, reason);

	}

	/// <remarks>
	/// Checks # of invaders in land 1st time it is called.
	/// Calls Token.Destroy
	/// </remarks>
	/// This => Base.DestroyNInvaders => Token.Destory => Tokens.Remove => This
	protected override async Task<int> DestroyNInvaders( HumanToken invaderToDestroy, int countToDestroy ) {
		countToDestroy = Math.Min( countToDestroy, this[invaderToDestroy] );
		if(countToDestroy == 0) throw new InvalidOperationException( "Can't destroy invaders because they aren't here." );

		if(PreviouslyDestroyed 
			|| InvaderCount != 1
		) {
			// track that we are destroying something
			ActionScope.Current[destroyedAnInvaderKey] = true;
			return await base.DestroyNInvaders( invaderToDestroy, countToDestroy );
		} else {
			// abduct it
			await AbductInvader( invaderToDestroy );
			return 0;
		}
	}

	async Task AbductInvader( HumanToken invaderToDestroy ) {
		var spaceTokenToRemove = invaderToDestroy.On(this);

		var invaderToAddToEndlessDark = ActionScope.Current.SafeGet<HumanToken>( damagedAnInvaderKey, invaderToDestroy );

		if(invaderToAddToEndlessDark == invaderToDestroy)
			await spaceTokenToRemove.MoveTo( EndlessDark.Space.ScopeSpace );
		else {
			await spaceTokenToRemove.Remove();
			await EndlessDark.Space.ScopeSpace.AddAsync( invaderToAddToEndlessDark, 1 );
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