namespace SpiritIsland;

/// <summary>
/// Binds to an ActionScope so work can be performed.
/// Knows Terrain
/// </summary>
public class ActionableSpaceState : SpaceState {

	public ActionableSpaceState( SpaceState spaceState ):base(spaceState) {}

	#region Adjust Health

	// It is questionable if this should be here since adjusting shouldn't make any difference
	// but in this case, it COULD destroy a token.

	public async Task AdjustHealthOfAll( int delta, params HumanTokenClass[] tokenClasses ) {
		if(delta == 0) return;
		foreach(var tokenClass in tokenClasses) {
			var tokens = OfHumanClass( tokenClass );
			var orderedTokens = delta < 0
				? tokens.OrderBy( x => x.FullHealth ).ToArray()
				: tokens.OrderByDescending( x => x.FullHealth ).ToArray();
			foreach(var token in orderedTokens)
				await AdjustHealthOf( token, delta, this[token] );
		}
	}

	/// <summary> Replaces (via adjust) HealthToken with new HealthTokens </summary>
	/// <returns> The # of remaining Adjusted tokens. </returns>
	public async Task<(HumanToken, int)> AdjustHealthOf( HumanToken token, int delta, int count ) {
		count = Math.Min( this[token], count );
		if(count == 0) return (token, 0);

		var newToken = token.AddHealth( delta ); // throws exception if health < 1

		if(newToken.IsDestroyed) {
			await Destroy( token, count ); // destroy the old token
			return (token, 0);
		}

		Adjust( token, -count );
		Adjust( newToken, count );
		return (newToken, count);
	}


	#endregion


	public Task AddDefault( HumanTokenClass tokenClass, int count, AddReason addReason = AddReason.Added )
		=> Add( GetDefault( tokenClass ), count, addReason );


	// Convenience only
	public Task Destroy( IVisibleToken token, int count ) => token is HumanToken ht
		? ht.Destroy( this, count )
		: Remove( token, count, RemoveReason.Destroyed );

	public async Task<TokenAddedArgs> Add( IVisibleToken token, int count, AddReason addReason = AddReason.Added ) {
		TokenAddedArgs addResult = Add_Silent( token, count, addReason );
		if(addResult != null) {
			addResult.GameState = _gameState;
			foreach(var handler in Keys.OfType<IHandleTokenAdded>().ToArray())
				await handler.HandleTokenAdded( addResult );
		}
		return addResult;
	}

	TokenAddedArgs Add_Silent( IVisibleToken token, int count, AddReason addReason = AddReason.Added ) {
		if(count < 0) throw new ArgumentOutOfRangeException( nameof( count ) );

		// Pre-Add check/adjust
		var addingArgs = new AddingTokenArgs( this, addReason ) { Count = count, Token = token };
		foreach(var mod in Keys.OfType<IHandleAddingToken>().ToArray())
			mod.ModifyAdding( addingArgs );

		if(addingArgs.Count < 0) throw new IndexOutOfRangeException( nameof( addingArgs.Count ) );
		if(addingArgs.Count == 0) return null;

		// Do Add
		Adjust(addingArgs.Token, addingArgs.Count);

		// Post-Add event
		return new TokenAddedArgs( this, addingArgs.Token, addReason, addingArgs.Count );
	}


	/// <summary> returns null if no token removed </summary>
	public virtual async Task<TokenRemovedArgs> Remove( IVisibleToken token, int count, RemoveReason reason = RemoveReason.Removed ) {

		// grab event handlers BEFORE the token is removed, so token can self-handle its own removal
		var tokenRemovedHandlers = Keys.OfType<IHandleTokenRemoved>().ToArray();

		var e = await Remove_Silent( token, count, reason );
		if(e == null) return null;

		foreach(IHandleTokenRemoved handler in tokenRemovedHandlers)
			await handler.HandleTokenRemoved( e );

		return e;
	}

	/// <summary> returns null if no token removed. Does Not publish event.</summary>
	protected async Task<TokenRemovedArgs> Remove_Silent( IVisibleToken token, int count, RemoveReason reason = RemoveReason.Removed ) {
		count = System.Math.Min( count, this[token] );

		// Pre-Remove check/adjust
		var removingArgs = new RemovingTokenArgs( this, reason ) { Count = count, Token = token };
		foreach(var mod in Keys.OfType<IHandleRemovingToken>().ToArray())
			await mod.ModifyRemoving( removingArgs );

		if(removingArgs.Count < 0) throw new IndexOutOfRangeException( nameof( removingArgs.Count ) );

		if(removingArgs.Count == 0) return null;

		// Do Remove
		Adjust(removingArgs.Token, -removingArgs.Count);

		// Post-Remove event
		return new TokenRemovedArgs( removingArgs.Token, reason, this, removingArgs.Count );

	}

	public async Task AddStrifeTo( HumanToken invader, int count = 1 ) {

		// Remove old type from 
		if(this[invader] < count)
			throw new ArgumentOutOfRangeException( $"collection does not contain {count} {invader}" );
		Adjust(invader, -count);

		// Add new strifed
		var strifed = invader.HavingStrife( invader.StrifeCount + 1 );
		Adjust(strifed, count);

		// !!! Adding / Removing a strife needs to trigger a token-change event for Observe the Ever Changing World
		// !!! Test that a ravage that does nothing but removes a strife, triggers Observe the Ever Changing World

		if(strifed.IsDestroyed) // due to a strife-health penalty
			await Destroy( strifed, this[strifed] );
	}

	/// <summary> Gathering / Pushing + a few others </summary>
	public async Task MoveTo( IVisibleToken token, Space destination ) {
		// Current implementation favors:
		//		switching token types prior to Add/Remove so events handlers don't switch token type
		//		perfoming the add/remove action After the Adding/Removing modifications

		// Possible problems with this method:
		//		The token in the Added event, may be different than token that was attempted to be added.
		//		The Token in the Removed event, may be a different token than was requested to be removed.
		//		The token Added may be Different than the token Removed
		//		If the Adding stops the and, what do we do about the token that was removed?
		//		Move requires a special Publish because it pertains to 2 spaces - we don't want to publish it twice (once for each space)

		// Possible solutions:
		//		Don't allow Adding to modify count
		//		Move has 2 tokens, token added and token removed

		if(this[token] == 0) return; // unable to remove desired token

		// Remove from source
		var removeResults = await Remove( token, 1, RemoveReason.MovedFrom );
		if(removeResults is null) return; // can be prevented by Remove-Mods

		// Add to destination
		var dstTokens = _api.GetTokensFor( destination );
		var addResult = await dstTokens.BindScope().Add( token, removeResults.Count, AddReason.MovedTo );
		if(addResult == null) return;

		// Publish
		await _api.Publish_Moved( new TokenMovedArgs( this, dstTokens ) { // !! _original ????
			// removed
			TokenRemoved = removeResults.Token,
			// added
			TokenAdded = addResult.Token,
			// general
			Count = 1,
		} );

	}

	public virtual HumanToken GetNewDamagedToken( HumanToken invaderToken, int availableDamage ) 
		=> invaderToken.AddDamage( availableDamage );
	public virtual Task<int> DestroyNTokens( HumanToken invaderToDestroy, int countToDestroy ) {
		return invaderToDestroy.Destroy( this, countToDestroy );
	}

	// Hopefully this is never called.
	// But if it is, do something reasonable
	public override ActionableSpaceState BindScope() => this;

}