namespace SpiritIsland;

public static class DamageInvader_Extensions {

	/// <returns>Damage inflicted.</returns>
	static public async Task<int> DoDamageAsync( this SourceSelector ss, Spirit spirit, int damage, Present present = Present.Done ) {
		if(damage == 0) return 0;

		var itemsToDamage = ss
			.GetEnumerator(spirit, Prompt.RemainingCount("Damage"), present, maxCount:damage );

		int damageInflicted = 0;
		await foreach(SpaceToken st in itemsToDamage) {
			await st.Space.Invaders.ApplyDamageTo1( 1, st.Token.AsHuman() );
			++damageInflicted;
		}

		return damageInflicted;
	}

}
