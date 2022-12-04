namespace SpiritIsland;

public class HealthToken : Token, IEquatable<HealthToken> {

	public HealthToken( 
		HealthTokenClass tokenClass, 
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

	public HealthTokenClass Class { get; }
	TokenClass Token.Class => this.Class;

	/// <summary>The effective FullHealth of a token. Minimum of 1; </summary>
	public int FullHealth => Math.Max(1, _rawFullHealth - StrifeCount * _healthPenaltyHolder.HealthPenaltyPerStrife );
	readonly int _rawFullHealth; // the value adjusted by modifications, may be less than 1.

	public int Damage { get; }
	public int DreamDamage { get; }
	
	public int FullDamage => Damage + DreamDamage;

	public int StrifeCount { get; }

	public bool IsDestroyed => FullHealth <= FullDamage;

	#region Token mutation generators

	public HealthToken HavingStrife(int strifeCount) {
		return strifeCount < 0 ? throw new System.ArgumentOutOfRangeException(nameof(strifeCount),$"strife Count = {strifeCount}")
			: strifeCount == StrifeCount ? this
			: new HealthToken( Class, _healthPenaltyHolder, _rawFullHealth, Damage, strifeCount );
	}

	/// <returns>a new token with the adjusted strife</returns>
	public HealthToken AddStrife( int deltaStrife ) => HavingStrife( StrifeCount + deltaStrife );

	public HealthToken AddDamage( int damage, int nightmareDamage=0 ) {
		int newDamage = Math.Min( Damage + damage, _rawFullHealth ); // Give regular damage priority
		// only allow nightmare damage to take up whatever remaining health is available
		int newNightmareDamage = Math.Min( nightmareDamage + DreamDamage, _rawFullHealth-newDamage );
		return new HealthToken( 
			Class, 
			_healthPenaltyHolder, 
			_rawFullHealth, 
			newDamage, 
			StrifeCount,
			newNightmareDamage
		);
	}

	// For Dream a 1000 Deaths, make token not in the Invader TokenCategory
	public HealthToken SwitchClass( HealthTokenClass newClass )
		=> new HealthToken( newClass, _healthPenaltyHolder, _rawFullHealth, Damage, StrifeCount, DreamDamage );

	public HealthToken Healthy => new HealthToken( Class, _healthPenaltyHolder, _rawFullHealth, 0, StrifeCount );

	public HealthToken AddHealth( int delta ) {
		int newHealth = Math.Max(1, _rawFullHealth + delta );
		return newHealth == _rawFullHealth ? this
			: new HealthToken( Class, _healthPenaltyHolder, newHealth, Damage, StrifeCount );
	}

	#endregion

	#region GetHashCode / Equals

	public override int GetHashCode() 
		=> Class.GetHashCode()
		+ 2 * _rawFullHealth	// Do NOT use FullHealth because that might change based on HealthPenalty
		+ 3 * Damage
		+ 5 * DreamDamage
		+ 7 * StrifeCount;

	public override bool Equals( object obj ) => this.Equals( obj as HealthToken );

	public bool Equals( HealthToken other ) {
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

	readonly string _summaryString;

	readonly IHaveHealthPenaltyPerStrife _healthPenaltyHolder;
}

public interface IHaveHealthPenaltyPerStrife {
	int HealthPenaltyPerStrife { get; }
}
