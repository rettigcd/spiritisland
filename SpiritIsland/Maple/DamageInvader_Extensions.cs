namespace SpiritIsland;

public static class DamageInvader_Extensions {

	/// <summary>
	/// Adds Mods to Spirit-Power Damage.  (Badlands damage has already been added.)
	/// </summary>
	/// <param name="space"></param>
	/// <param name="damagePicker"></param>
	/// <param name="damage"></param>
	/// <param name="allowedTypes"></param>
	/// <returns></returns>
	static public async Task<int> UserSelected_DamageInvadersAsync( this Space space, Spirit damagePicker, int damage, params ITokenClass[] allowedTypes) {
		if( allowedTypes.Length == 0 ) allowedTypes = Human.Invader;

		var args = new DamageFromSpiritPowers { Space = space, Classes = allowedTypes, Damage = damage };
		var mods = space.ModsOfType<IAdjustDamageToInvaders_FromSpiritPowers>().ToArray();
		foreach( var mod in mods )
			await mod.ModifyDamage(args);

		var quota = new DamageQuota_NoMods(args.Damage,allowedTypes);
		await space.SourceSelector
			.UseQuota(quota)
			.DoDamageAsync(damagePicker, args.Damage, Present.Always);
		return quota.AppliedDamage;
	}

	/// <returns>Damage inflicted.</returns>
	static public async Task<int> DoDamageAsync_Old( this SourceSelector ss, Spirit spirit, int damage, Present present = Present.Done ) {
		if(damage == 0) return 0;

		IAsyncEnumerable<SpaceToken> itemsToDamage = ss
			.GetEnumerator(spirit, Prompt.RemainingCount("Damage"), present, maxCount:damage );

		int damageInflicted = 0;
		await foreach(SpaceToken st in itemsToDamage) {
			await st.Space.Invaders.ApplyDamageTo1( 1, st.Token.AsHuman() );
			++damageInflicted;
		}

		return damageInflicted;
	}

	/// <returns>Damage inflicted.</returns>
	static public async Task DoDamageAsync(this SourceSelector ss, Spirit spirit, int damage, Present present = Present.Done) {

		// !!! remove the damage parameter.

		var itemsToDamage = ss.GetEnumerator(spirit, Prompt.RemainingCount("Damage"), present, maxCount: damage);
		await foreach(SpaceToken st in itemsToDamage )
			await st.Space.Invaders.ApplyDamageTo1(1, st.Token.AsHuman());
	}

}

public class DamageQuota_NoMods : IQuota {

	public int AppliedDamage => _appliedDamage;

	#region constructor

	public DamageQuota_NoMods(int damage) {
		_maxDamage = damage;
		_allowedTypes = [..Human.Invader];
	}

	public DamageQuota_NoMods(int damage, ITokenClass[] allowedTypes) {
		_maxDamage = damage;
		_allowedTypes = allowedTypes;
	}

	#endregion constructor

	public IEnumerable<SpaceToken> GetSourceOptionsOn1Space(Space sourceSpace) {
		return _appliedDamage <= _maxDamage 
			? sourceSpace.HumanOfTag(TokenCategory.Invader).On(sourceSpace) 
			: Array.Empty<SpaceToken>();
	}

	public void MarkTokenUsed(ITokenLocation token) {
		++_appliedDamage;
	}

	public string RemainingTokenDescriptionOn(Space[] sourceSpaces) {
		return sourceSpaces.Sum(s => s.SumAny(_allowedTypes)).ToString();
	}

	int _appliedDamage = 0;
	readonly int _maxDamage;
	readonly ITokenClass[] _allowedTypes;
}


public class DamageQuota2 : IQuota {

	public int AppliedDamage => _appliedDamage;

	#region constructor

	public DamageQuota2(int damage) {
		_maxDamage = damage;
		_allowedTypes = [..Human.Invader];
	}

	public DamageQuota2(int damage, ITokenClass[] allowedTypes) {
		_maxDamage = damage;
		_allowedTypes = allowedTypes;
	}

	#endregion constructor

	public IEnumerable<SpaceToken> GetSourceOptionsOn1Space(Space sourceSpace) {

		if( _appliedDamage <= _maxDamage )
			return sourceSpace.HumanOfTag(TokenCategory.Invader).On(sourceSpace);



		return Array.Empty<SpaceToken>();
	}

	readonly Dictionary<Space,int> _bonusCounts = new();

	public void MarkTokenUsed(ITokenLocation tokenLocation) {
		++_appliedDamage;
		if(tokenLocation.Location is Space space && !_bonusCounts.ContainsKey(space) 
		) {
			var args = new DamageFromSpiritPowers { Space = space, Classes = _allowedTypes, Damage = 1 };
			var mods = space.ModsOfType<IAdjustDamageToInvaders_FromSpiritPowers>().ToArray();
			foreach( var mod in mods )
				mod.ModifyDamage(args);
			_bonusCounts[space] = args.Damage-1;
		}
	}

	public string RemainingTokenDescriptionOn(Space[] sourceSpaces) {
		return sourceSpaces.Sum(s => s.SumAny(_allowedTypes)).ToString();
	}

	int _appliedDamage = 0;
	readonly int _maxDamage;
	readonly ITokenClass[] _allowedTypes;
}
