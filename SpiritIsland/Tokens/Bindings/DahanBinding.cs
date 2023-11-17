namespace SpiritIsland;

public sealed class DahanBinding {

	readonly SpaceState _tokens;

	public DahanBinding( SpaceState tokens ) {
		_tokens = tokens;
	}

	#region All TokenCategory.Dahan, including Frozen/Stasis

	/// <summary> Includes Frozen and dream dahan. </summary>
	public bool Any => _tokens.Has( TokenCategory.Dahan );
	public int CountAll => _tokens.Sum( TokenCategory.Dahan );

	#endregion

	/// <summary> The non-frozes,non-dreaming Tokens of various health and damage.</summary>
	public HumanToken[] NormalKeys => _tokens.HumanOfTag( Human.Dahan );

	public void Init( int count ) => _tokens.InitDefault( Human.Dahan, count );

	public void Adjust( ISpaceEntity token, int delta ) => _tokens.Adjust( token, delta );

	/// <summary> Adds a Token from the bag, or out of thin air. </summary>
	public Task AddDefault( int count, AddReason reason = AddReason.Added )
		=> _tokens.AddDefault( Human.Dahan, count, reason );

	// Called from .Move() and .Dissolve the Bonds
	public async Task<TokenRemovedArgs> Remove1( IToken toRemove, RemoveReason reason ) {
		return await _tokens.Remove( toRemove, 1, reason );
	}

	#region Higher Level of Abstraction

	/// <summary>Automated - Applies Damage Inefficiently, from power</summary>
	public async Task<int> ApplyDamage_Inefficiently( int remainingDamageToDahan ) {

		// From BAC Rulebook p.16
		// When Spirit Powers Damage the Dahan,
		// you may choose how that Damage is allocated, just like when you Damage Invaders.

		int totalDamageUsed = 0;

		HumanToken mostHealthy = null;
		while(0 < remainingDamageToDahan
			// find the most healthy dahan we have
			&& (mostHealthy = NormalKeys.OrderByDescending( x => x.RemainingHealth ).FirstOrDefault()) != null // least health first.
		) {
			// determine # to apply 1 damage to
			int countToApply1DamageTo = Math.Min( remainingDamageToDahan, _tokens[mostHealthy] );

			// Damage 1 to specific token
			await DamageNTokens( mostHealthy, countToApply1DamageTo, 1 );

			remainingDamageToDahan -= countToApply1DamageTo;
			totalDamageUsed += countToApply1DamageTo;
		}

		return totalDamageUsed;
	}

	public async Task Apply1DamageToAll() { // Called By Power (i.e. not invaders)

		var before = NormalKeys
			.OrderByDescending( x => x.Damage ) // most damaged to least damaged so they don't cascade
			.ToArray();

		foreach(var token in before)
			await DamageNTokens( token, _tokens[token], 1 );
	}

	/// <summary>Ravage - Applies Damage Efficiently To as many tokens of the given type as there are.</summary>
	/// <returns>Remaining/unused damage</returns>
	public async Task<int> ApplyDamageToAll_Efficiently( int remainingDamageToDahan, HumanToken token ) {
		// Destroy what can be destroyed
		if(token.RemainingHealth <= remainingDamageToDahan) {
			int countWeCouldDestroy = remainingDamageToDahan / token.RemainingHealth;
			int countDestroyed = Math.Min( countWeCouldDestroy, _tokens[token] );
			await DamageNTokens( token, countDestroyed, token.RemainingHealth );
			remainingDamageToDahan -= countDestroyed * token.RemainingHealth;
		}

		// if there is still partial damage we can apply
		if(0 < remainingDamageToDahan && 0 < _tokens[token]) {

			// We should change 'Saved' dahan to Saved-Dahan class so that they don't show up in the above 0 < _tokens[token] check
			// However, we are not currently doing that, so we need could land in here
			// and still have more damage available than a full token can take
			if( remainingDamageToDahan < token.RemainingHealth) {
				// apply the left-over damage to 1 token.
				await DamageNTokens( token, 1, remainingDamageToDahan );
				remainingDamageToDahan = 0; // damage should be used up
			} else {
				// the remaining dahan are Saved-Dahan, and shan't take anymore damage.
			}

		}

		return remainingDamageToDahan;
	}

	#endregion Higher Level of Abstraction

	#region Damage

	public async Task DamageNTokens( HumanToken originalToken, int tokenCountToReceiveDamage, int damagePerToken ) {

		var notification = new DamagingTokens(_tokens) {  
			Token = originalToken, 
			TokenCountToReceiveDamage = tokenCountToReceiveDamage, 
			DamagePerToken = damagePerToken
		};
		var damageMods = _tokens.ModsOfType<IModifyDahanDamage>().ToArray();
		foreach(IModifyDahanDamage mod in damageMods) 
			mod.Modify( notification );
		originalToken = notification.Token;
		tokenCountToReceiveDamage = notification.TokenCountToReceiveDamage;
        damagePerToken = notification.DamagePerToken;
		if(tokenCountToReceiveDamage == 0 || damagePerToken == 0) return;

		// Not cleaning up the data because:
		// we want the caller to know exactly how much damage they are doing before they call this method
		// so that this method does not have to return any damage-used / damage-remaining results.
		if(_tokens[originalToken] < tokenCountToReceiveDamage) 
			throw new InvalidOperationException($"Can't damage {tokenCountToReceiveDamage} because there are only {_tokens[originalToken]} available {originalToken.HumanClass.Label}.");
		if(originalToken.RemainingHealth < damagePerToken)
			throw new InvalidOperationException( $"Can't damage apply {damagePerToken} because they only have {originalToken.RemainingHealth}." );

		HumanToken damagedToken = originalToken.AddDamage( damagePerToken );
		if(damagedToken.IsDestroyed) {
			await originalToken.Destroy( _tokens, tokenCountToReceiveDamage );
		} else {
			_tokens.Adjust( originalToken, -tokenCountToReceiveDamage );
			_tokens.Adjust( damagedToken, tokenCountToReceiveDamage );
		}
	}

	#endregion

	#region Destroy

	// This destroy is using Least-Efficient (dahan-only)
	// for Invaders, we would want this to be MOST efficient
	public async Task Destroy( int countToDestroy ) {

		var before = NormalKeys
			.OrderBy( x => x.RemainingHealth )
			.ToArray(); // least health first.
		foreach(var token in before) {
			// Destroy what can be destroyed
			int destroyCountToApplyToThisToken = Math.Min( countToDestroy, _tokens[token] );
			countToDestroy -= destroyCountToApplyToThisToken; // doesn't matter how many were actually destroyed.
			await Destroy(destroyCountToApplyToThisToken, token);
		}
	}

	// Only called externally by Ravage
	/// <returns># of tokens destroyed</returns>
	public async Task<int> Destroy( int count, HumanToken token ) {
		// ! Running this through the Dahan.Destroy is unnessary since it is never overriden.
		// However, this matches the Invader pattern which IS necessary due to Habsburg Durable tokens.
		int x = Math.Min( count, _tokens[token] );
		return await token.Destroy( _tokens, x );
	}

	public async Task DestroyAll() {
		foreach(HumanToken token in NormalKeys.ToArray())
			await token.DestroyAll( _tokens );
	}

	#endregion

}

public class DamagingTokens {
	public DamagingTokens( SpaceState on ) {
		On = on;
	}
	public SpaceState On { get; } 
	public HumanToken Token { get; set; }
	public int TokenCountToReceiveDamage { get; set; }
	public int DamagePerToken { get; set; }
}
