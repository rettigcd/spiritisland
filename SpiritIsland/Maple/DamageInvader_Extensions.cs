namespace SpiritIsland;

public static class DamageInvader_Extensions {

	static public async Task<int> UserSelected_DamageInvadersAsync( this Space space, Spirit damagePicker, int damage, params ITokenClass[] allowedTypes) {
		if( allowedTypes.Length == 0 ) allowedTypes = Human.Invader;

		var args = new DamageFromSpiritPowers { Space = space, Classes = allowedTypes, Damage = damage };
		var mods = space.ModsOfType<IAdjustDamageToInvaders_FromSpiritPowers>().ToArray();
		foreach( var mod in mods )
			await mod.ModifyDamage(args);

		return await space.SourceSelector
			.UseQuota(new Quota().AddAll(allowedTypes))
			.DoDamageAsync(damagePicker, args.Damage, Present.Always);
	}

	/// <returns>Damage inflicted.</returns>
	static public async Task<int> DoDamageAsync( this SourceSelector ss, Spirit spirit, int damage, Present present = Present.Done ) {
		if(damage == 0) return 0;

		IAsyncEnumerable<SpaceToken> itemsToDamage = ss.GetEnumerator(spirit, Prompt.RemainingCount("Damage"), present, maxCount:damage );

		int damageInflicted = 0;
		await foreach(SpaceToken st in itemsToDamage) {
			await st.Space.Invaders.ApplyDamageTo1( 1, st.Token.AsHuman() );
			++damageInflicted;
		}

		return damageInflicted;
	}

}

//
//
// Multi-Space damager => Source Selector
//
//	Get Token
//		if 1st time in space, 
//	
//	

