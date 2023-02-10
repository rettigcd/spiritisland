namespace SpiritIsland;

public class HumanToken : IToken, IAppearInSpaceAbreviation, IEquatable<HumanToken> {

	public HumanToken( 
		HumanTokenClass tokenClass, 
		IHaveHealthPenaltyPerStrife penaltyHolder, 
		int rawFullHealth, 
		int damage = 0, 
		int strifeCount = 0,
		int nightmareDamage = 0
	) {

		Class = tokenClass;
		_rawFullHealth = rawFullHealth;

		Damage = damage;
		DreamDamage = nightmareDamage;
		StrifeCount = strifeCount;

		_healthPenaltyHolder = penaltyHolder;

		_summaryString = Class.Initial + "@" + RemainingHealth
			+ (DreamDamage == 0 ? "" : new string( '~', DreamDamage))
			+ (strifeCount == 0 ? "" : new string( '^', StrifeCount ));
	}

	public HumanTokenClass Class { get; }
	IEntityClass ISpaceEntity.Class => this.Class;

	/// <summary>The effective FullHealth of a token. Minimum of 1; </summary>
	public int FullHealth => Math.Max(1, _rawFullHealth - StrifeCount * _healthPenaltyHolder.HealthPenaltyPerStrife );
	readonly int _rawFullHealth; // the value adjusted by modifications, may be less than 1.

	public int Damage { get; }
	public int DreamDamage { get; }
	
	public int FullDamage => Damage + DreamDamage;

	public int StrifeCount { get; }

	public bool IsDestroyed => FullHealth <= FullDamage;

	public virtual async Task<int> Destroy( SpaceState tokens, int count ) {
		count = Math.Min(count, tokens[this]);
		var result = await tokens.Remove( this, count, RemoveReason.Destroyed );
		if(result is null) return 0; // maybe SpaceState prevented their removal.
		GameState.Current.Fear.AddDirect( new FearArgs( this.Class.FearGeneratedWhenDestroyed * count ) {
			FromDestroyedInvaders = true, // this is the destruction that Dread Apparitions ignores.
			space = tokens.Space
		} );
		return result.Count;
	}

	public virtual async Task<int> DestroyAll( SpaceState tokens ) {
		int count = tokens[this];
		var result = await tokens.Remove( this, count, RemoveReason.Destroyed );
		if(result is null) return 0; // maybe SpaceState prevented their removal.
		GameState.Current.Fear.AddDirect( new FearArgs( this.Class.FearGeneratedWhenDestroyed * count ) {
			FromDestroyedInvaders = true, // this is the destruction that Dread Apparitions ignores.
			space = tokens.Space
		} );
		return result.Count;
	}


	#region Token mutation generators

	// so we can override it in derived types and not switch back to HealthToken base
	protected virtual HumanToken NewHealthToken(
		HumanTokenClass tokenClass,
		IHaveHealthPenaltyPerStrife penaltyHolder,
		int rawFullHealth,
		int damage = 0,
		int strifeCount = 0,
		int nightmareDamage = 0
	) => new HumanToken( tokenClass, penaltyHolder, rawFullHealth, damage, strifeCount, nightmareDamage );

	public HumanToken HavingStrife(int strifeCount) {
		return strifeCount < 0 ? throw new System.ArgumentOutOfRangeException(nameof(strifeCount),$"strife Count = {strifeCount}")
			: strifeCount == StrifeCount ? this
			: NewHealthToken( Class, _healthPenaltyHolder, _rawFullHealth, Damage, strifeCount );
	}

	/// <returns>a new token with the adjusted strife</returns>
	public HumanToken AddStrife( int deltaStrife ) => HavingStrife( StrifeCount + deltaStrife );

	public HumanToken AddDamage( int damage, int nightmareDamage=0 ) {
		int newDamage = Math.Min( Damage + damage, _rawFullHealth ); // Give regular damage priority
		// only allow nightmare damage to take up whatever remaining health is available
		int newNightmareDamage = Math.Min( nightmareDamage + DreamDamage, _rawFullHealth-newDamage );
		return NewHealthToken( Class, _healthPenaltyHolder, _rawFullHealth, newDamage, StrifeCount,	newNightmareDamage );
	}

	// For Dream a 1000 Deaths, make token not in the Invader TokenCategory
	public HumanToken SwitchClass( HumanTokenClass newClass )
		=> NewHealthToken( newClass, _healthPenaltyHolder, _rawFullHealth, Damage, StrifeCount, DreamDamage );

	public HumanToken Healthy 
		=> NewHealthToken( Class, _healthPenaltyHolder, _rawFullHealth, 0, StrifeCount );

	public HumanToken AddHealth( int delta ) {
		int newHealth = Math.Max(1, _rawFullHealth + delta );
		return newHealth == _rawFullHealth ? this
			: NewHealthToken( Class, _healthPenaltyHolder, newHealth, Damage, StrifeCount );
	}

	#endregion

	#region GetHashCode / Equals

	public override int GetHashCode() 
		=> Class.GetHashCode()
		+ 2 * _rawFullHealth	// Do NOT use FullHealth because that might change based on HealthPenalty
		+ 3 * Damage
		+ 5 * DreamDamage
		+ 7 * StrifeCount;

	public override bool Equals( object obj ) => this.Equals( obj as HumanToken );

	public bool Equals( HumanToken other ) {
		return other != null
			&& other.Class == Class
			&& other._rawFullHealth == _rawFullHealth
			&& other.Damage == Damage	
			&& other.StrifeCount == StrifeCount;
	}

	#endregion

	public int RemainingHealth => FullHealth - FullDamage;

	public override string ToString() => _summaryString;

	string IOption.Text => _summaryString;

	public string SpaceAbreviation => _summaryString;

	public Img Img => Class.Img;

	readonly string _summaryString;

	readonly public IHaveHealthPenaltyPerStrife _healthPenaltyHolder;
}
