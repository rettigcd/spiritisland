namespace SpiritIsland;

public sealed class InvaderBinding( Space space ) {

	#region constructor

	#endregion

	public readonly Space Space = space;

	#region Apply Damage To...

	/// <summary> Not Badland-aware </summary>
	public async Task ApplyDamageToEach( int individualDamage, params ITokenClass[] generic ) {
		if(Space.ModsOfType<IAdjustDamageToInvaders_ByStoppingIt>().Any()) return;

		var invaders = Space.InvaderTokens()
			.OrderBy(x=>x.RemainingHealth) // do damaged first to clear them out
			.ToArray();

		// Filter if appropriate
		if(generic != null && 0<generic.Length)
			invaders = invaders.Where(t=>generic.Contains(t.HumanClass)).ToArray();

		foreach(var invader in invaders)
			for(int num = Space[invader]; num>0; --num) // can't use while this[invader]>0 because BoD doesn't actually destroy them.
				await ApplyDamageTo1( individualDamage, invader );

	}

	/// <summary> Not Badland-aware </summary>
	/// <returns>(damage inflicted,damagedInvader)</returns>
	public async Task<(int,HumanToken)> ApplyDamageTo1( int availableDamage, HumanToken originalInvader ) {
		if(Space[originalInvader] < 1)
			throw new InvalidOperationException( $"Cannot remove 1 {originalInvader} tokens because there aren't that many." );

		if(Space.ModsOfType<IAdjustDamageToInvaders_ByStoppingIt>().Any()) return (0,originalInvader);

		//!!! can we clean this up
		var damagedInvader = originalInvader.AddDamage(availableDamage);

		if(!damagedInvader.IsDestroyed) {
			Space.Humans(1, originalInvader).Adjust(_ => damagedInvader);

			var damagedHandlers = Space.ModsOfType<IHandleInvaderDamaged>().ToArray();
			foreach( IHandleInvaderDamaged handler in damagedHandlers)
				handler.HandleDamage(originalInvader, damagedInvader, Space);

		} else {
			var result = await Space.RemoveAsync( originalInvader, 1, DestroyingFromDamage.TriggerReason );
			await Space.AddFear(
				originalInvader.HumanClass.FearGeneratedWhenDestroyed * result.Count,
				FearType.FromInvaderDestruction // this is the destruction that Dread Apparitions ignores.
			);
		}

		int damageInflicted = originalInvader.RemainingHealth - damagedInvader.RemainingHealth;
		return (damageInflicted, damagedInvader);
	}

	#endregion

	#region Destroy

	public async Task DestroyAll( params HumanTokenClass[] tokenClasses ) {
		if(Space.ModsOfType<IAdjustDamageToInvaders_ByStoppingIt>().Any()) return;

		var tokensToDestroy = Space.HumanOfAnyTag( tokenClasses ).ToArray();
		foreach(var token in tokensToDestroy)
			await Space.DestroyAll( token );
	}

	public async Task DestroyNOfAnyClass( int count, params HumanTokenClass[] generics ) {
		if(Space.ModsOfType<IAdjustDamageToInvaders_ByStoppingIt>().Any()) return;

		HumanToken[] invadersToDestroy;
		while(
			0 < count 
			&& 0 < ( invadersToDestroy = [..Space.HumanOfAnyTag( generics )] ).Length
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
		if(Space.ModsOfType<IAdjustDamageToInvaders_ByStoppingIt>().Any()) return 0;

		countToDestroy = Math.Min( countToDestroy, Space.Sum( invaderClass ) );
		int remaining = countToDestroy; // capture

		while(0 < remaining) {
			var next = Space.HumanOfTag( invaderClass )
				.OrderByDescending( x => x.FullHealth )
				.ThenBy( x => x.StrifeCount )
				.ThenBy( x => x.Damage )
				.First();
			int countOfTypeToDestroy = Math.Min( remaining, Space[next]);
			await DestroyNTokens( next, countOfTypeToDestroy );
			remaining -= countOfTypeToDestroy;
		}

		return countToDestroy;
	}

	// destroy TOKEN
	public async Task DestroyNTokens( HumanToken invaderToDestroy, int countToDestroy ) {
		if(Space.PreventsInvaderDamage()) return;
		await Space.Destroy(invaderToDestroy, countToDestroy );
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
		if(Space.SumAny(removables) == 0) return;

		var invaderToRemove = Space.BestInvaderToBeRidOf( removables );

		if(invaderToRemove != null)
			await Space.RemoveAsync( invaderToRemove, 1, reason );
	}

	public Task Remove( IToken token, int count, RemoveReason reason = RemoveReason.Removed )
		=> Space.RemoveAsync( token, count, reason );

	#endregion

}
