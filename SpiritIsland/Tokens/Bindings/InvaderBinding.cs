namespace SpiritIsland;

public sealed class InvaderBinding( SpaceState tokens ) {

	#region constructor

	#endregion

	public readonly SpaceState Tokens = tokens;

	#region Apply Damage To...

	/// <summary> Not Badland-aware </summary>
	public async Task ApplyDamageToEach( int individualDamage, params ITokenClass[] generic ) {
		if(Tokens.ModsOfType<IStopInvaderDamage>().Any()) return;

		var invaders = Tokens.InvaderTokens()
			.OrderBy(x=>x.RemainingHealth) // do damaged first to clear them out
			.ToArray();

		// Filter if appropriate
		if(generic != null && 0<generic.Length)
			invaders = invaders.Where(t=>generic.Contains(t.HumanClass)).ToArray();

		foreach(var invader in invaders)
			for(int num = Tokens[invader]; num>0; --num) // can't use while this[invader]>0 because BoD doesn't actually destroy them.
				await ApplyDamageTo1( individualDamage, invader );

	}

	/// <summary> Not Badland-aware </summary>
	/// <returns>(damage inflicted,damagedInvader)</returns>
	public async Task<(int,HumanToken)> ApplyDamageTo1( int availableDamage, HumanToken originalInvader ) {
		if(Tokens[originalInvader] < 1)
			throw new InvalidOperationException( $"Cannot remove 1 {originalInvader} tokens because there aren't that many." );

		if(Tokens.ModsOfType<IStopInvaderDamage>().Any()) return (0,originalInvader);

		//!!! can we clean this up
		var damagedInvader = Tokens.GetNewDamagedToken( originalInvader, availableDamage );

		if(!damagedInvader.IsDestroyed) {
			Tokens.Humans(1, originalInvader).Adjust(_ => damagedInvader);
			InvaderDamaged?.Invoke( originalInvader );
		} else {
			if(!Tokens.PreventsInvaderDamage()){ 
				var result = await Tokens.RemoveAsync( originalInvader, 1, DestroyingFromDamage.TriggerReason );
				Tokens.AddFear(
					originalInvader.HumanClass.FearGeneratedWhenDestroyed * result.Count,
					FearType.FromInvaderDestruction // this is the destruction that Dread Apparitions ignores.
				);
			}
		}

		int damageInflicted = originalInvader.RemainingHealth - damagedInvader.RemainingHealth;
		return (damageInflicted, damagedInvader);
	}

	// The invader Before it was damaged.
	public event Action<HumanToken> InvaderDamaged;

	#endregion

	#region Destroy

	public async Task DestroyAll( params HumanTokenClass[] tokenClasses ) {
		if(Tokens.ModsOfType<IStopInvaderDamage>().Any()) return;

		var tokensToDestroy = Tokens.HumanOfAnyTag( tokenClasses ).ToArray();
		foreach(var token in tokensToDestroy)
			await Tokens.DestroyAll( token );
	}

	public async Task DestroyNOfAnyClass( int count, params HumanTokenClass[] generics ) {
		if(Tokens.ModsOfType<IStopInvaderDamage>().Any()) return;

		HumanToken[] invadersToDestroy;
		while(
			0 < count 
			&& 0 < ( invadersToDestroy = [..Tokens.HumanOfAnyTag( generics )] ).Length
		) {
			var invader = invadersToDestroy
				.OrderByDescending( x => x.FullHealth )
				.First();
			await DestroyNOfClass( 1, invader.HumanClass );

			// next
			--count;
		}
	}

	// destroy CLASS
	public async Task<int> DestroyNOfClass( int countToDestroy, HumanTokenClass invaderClass ) {
		if(Tokens.ModsOfType<IStopInvaderDamage>().Any()) return 0;

		countToDestroy = Math.Min( countToDestroy, Tokens.Sum( invaderClass ) );
		int remaining = countToDestroy; // capture

		while(0 < remaining) {
			var next = Tokens.HumanOfTag( invaderClass )
				.OrderByDescending( x => x.FullHealth )
				.ThenBy( x => x.StrifeCount )
				.ThenBy( x => x.FullDamage )
				.First();
			int countOfTypeToDestroy = Math.Min( remaining, Tokens[next]);
			await DestroyNTokens( next, countOfTypeToDestroy );
			remaining -= countOfTypeToDestroy;
		}

		return countToDestroy;
	}

	// destroy TOKEN
	public async Task DestroyNTokens( HumanToken invaderToDestroy, int countToDestroy ) {
		if(Tokens.PreventsInvaderDamage()) return;
		await Tokens.Destroy(invaderToDestroy, countToDestroy );
	}
	#endregion Destroy

	#region Remove

	/// <remarks>
	/// This is neither damage nor destroy.
	/// It is Game-Aware in that it understands non-strifed invaders are more dangerous than non-strifed, so it doesn't belong in the generic TokenDictionary class.
	/// However, it also does not require any input from a user, so it should not be on a TargetSpaceCtx.
	/// Sticking on InvaderGroup is the only place I can think to put it.
	/// Also, shouldn't be affected by Bringer overwriting 'Destroy' and 'Damage'
	/// </remarks>
	public async Task RemoveLeastDesirable( RemoveReason reason = RemoveReason.Removed, params ITokenClass[] removables ) {
		if(Tokens.SumAny(removables) == 0) return;

		var invaderToRemove = Tokens.BestInvaderToBeRidOf( removables );

		if(invaderToRemove != null)
			await Tokens.RemoveAsync( invaderToRemove, 1, reason );
	}

	public Task Remove( IToken token, int count, RemoveReason reason = RemoveReason.Removed )
		=> Tokens.RemoveAsync( token, count, reason );

	#endregion

}
