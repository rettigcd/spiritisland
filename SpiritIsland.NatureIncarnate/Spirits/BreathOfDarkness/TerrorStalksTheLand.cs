namespace SpiritIsland.NatureIncarnate;

public class TerrorStalksTheLand : SpaceState {

	static public SpecialRule Rule => new SpecialRule( "Terror Stalks the Land", "You have an Incarna. You may Abduct 1 Explorer / Town at empowered Incarna each Fast Phase. To Abduct a piece, Move it to tThe Endless Dark.  When pieces Escape, Move them to a non-Ocean land with you r Presence/Incarna.  If they have no legal land to move to, you lose.  Whenyour Powers would directly damage or directly destroy the only Invader in a land, instead Abduct it." );

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
	public override async Task<int> DestroyNInvaders( HumanToken invaderToDestroy, int countToDestroy ) {
		countToDestroy = Math.Min( countToDestroy, this[invaderToDestroy] );
		if( countToDestroy == 0 ) throw new InvalidOperationException("Can't destroy invaders because they aren't here.");

		ActionScope scope = ActionScope.Current;
		const string destroyedAnInvaderKey = "DestroyedAnInvader";
		bool previouslyDestroyed = scope.SafeGet<bool>( destroyedAnInvaderKey, false );

		if(!previouslyDestroyed && this.SumAny(Human.Invader)==1){
			// abduct it
			await new SpaceToken(Space,invaderToDestroy).MoveTo(EndlessDark.Space);
			return 0;
		} else {
			// track that we are destroying something
			scope[destroyedAnInvaderKey] = true;
			return await base.DestroyNInvaders(invaderToDestroy, countToDestroy );
		}

	}

}