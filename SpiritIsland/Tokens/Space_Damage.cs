namespace SpiritIsland;

public partial class Space {

	// !!! Make sure that everything that calls this should NOT be including Badland damage
	/// <returns>Damage inflicted.</returns>
	public async Task<int> UserSelected_DamageInvadersAsync(Spirit damagePicker, int damage, params ITokenClass[] allowedTypes) {
		if( allowedTypes.Length == 0 ) allowedTypes = Human.Invader;

		var args = new DamageFromSpiritPowers { Space = this, Classes = allowedTypes, Damage = damage };
		var mods = ModsOfType<IAdjustDamageToInvaders_FromSpiritPowers>().ToArray();
		foreach(var mod in mods )
			await mod.ModifyDamage(args);

		return await SourceSelector
			.AddAll(allowedTypes)
			.DoDamageAsync(damagePicker, args.Damage, Present.Always);
	}

	// This is needed when strifed invaders ravage OTHER tokens. Need to be able to exclude specific token
	public async Task<int> UserSelected_ApplyDamageToSpecificTokenAsync(Spirit damagePicker, int damage, HumanToken source, Func<HumanToken[]> allowedTypes) {
		if( damage == 0 ) return 0;

		IToken[] options;
		int damageInflicted = 0;
		while( 0 < damage && (options = AllHumanTokens().Intersect(allowedTypes()).ToArray()).Length > 0 ) {
			var st = await damagePicker.Select(An.Invader.ForAggregateDamageFromSource(SpaceSpec, source, options, damage, Present.Always));
			if( st is null ) break;
			var invaderToDamage = st.Token.AsHuman();
			await Invaders.ApplyDamageTo1(1, invaderToDamage);
			--damage;
			++damageInflicted;
		}
		return damageInflicted;
	}

	/// <summary>
	/// Called from Veil... to strife different invaders.
	/// </summary>
	public async Task<int> UserSelected_ApplyDamageToSpecificTokensAsync(Spirit damagePicker, List<IToken> invaders, int additionalTotalDamage) {
		int done = 0;

		while( 0 < additionalTotalDamage ) {
			var st = await damagePicker.Select(An.Invader.ForBadlandDamage(additionalTotalDamage, invaders.On(this)));
			if( st is null ) break;
			var invader = st.Token.AsHuman();
			int index = invaders.IndexOf(invader);
			var (_, moreDamagedToken) = await Invaders.ApplyDamageTo1(1, invader);
			++done;
			if( 0 < moreDamagedToken.RemainingHealth )
				invaders[index] = moreDamagedToken;
			else
				invaders.RemoveAt(index);
		}
		return done;
	}


}
