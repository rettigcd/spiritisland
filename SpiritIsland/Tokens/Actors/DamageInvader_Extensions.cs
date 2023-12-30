namespace SpiritIsland;

public static class DamageInvader_Extensions {

	// !!! Make sure that everything that calls this should NOT be including Badland damage
	static public Task<int> UserSelected_DamageInvaders( this SpaceState tokens, Spirit damagePicker, int damage, params ITokenClass[] allowedTypes ) {
		if(allowedTypes.Length == 0) allowedTypes = Human.Invader;
		return tokens.SourceSelector
			.AddAll(allowedTypes)
			.DoDamage( damagePicker, damage, Present.Always );
	}

	// not required, does count
	static public async Task<int> DoDamage( this SourceSelector ss, Spirit spirit, int damage, Present present = Present.Done ) {
		if(damage == 0) return 0;

		var itemsToDamage = ss
			.GetEnumerator(spirit, Prompt.RemainingCount("Damage"), present, maxCount:damage );

		int damageInflicted = 0;
		await foreach(SpaceToken st in itemsToDamage) {
			await st.Space.Tokens.Invaders.ApplyDamageTo1( 1, st.Token.AsHuman() );
			++damageInflicted;
		}

		return damageInflicted;
	}

	// This is needed when strifed invaders ravage OTHER tokens. Need to be able to exclude specific token
	static public async Task<int> UserSelected_ApplyDamageToSpecificToken( this SpaceState tokens, Spirit damagePicker, int damage, HumanToken source, Func<HumanToken[]> allowedTypes ) {
		if(damage == 0) return 0;

		IToken[] options;
		int damageInflicted = 0;
		while(0 < damage && (options = tokens.Humans().Intersect( allowedTypes() ).ToArray()).Length > 0) {
			var st = await damagePicker.SelectAsync( An.Invader.ForAggregateDamageFromSource( tokens.Space, source, options, damage, Present.Always ) );
			if( st == null) break;
			var invaderToDamage = st.Token.AsHuman();
			await tokens.Invaders.ApplyDamageTo1( 1, invaderToDamage );
			--damage;
			++damageInflicted;
		}
		return damageInflicted;
	}


}
