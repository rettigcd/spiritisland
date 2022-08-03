namespace SpiritIsland;

public class HealthToken : Token, IEquatable<HealthToken> {

	public HealthToken( HealthTokenClass tokenClass, int rawFullHealth, int damage = 0, int strifeCount = 0 ) {

		Class = tokenClass;
		_rawFullHealth = rawFullHealth;

		Damage = damage;
		StrifeCount = strifeCount;

		_summaryString = Class.Initial + "@" + RemainingHealth
			+ (strifeCount == 0 ? "" : new string( '^', StrifeCount ));
	}

	public HealthTokenClass Class { get; }
	TokenClass Token.Class => this.Class;

	static public int HealthPenaltyPerStrife { get; set; } = 0; // !!! put this in the game state instead of static global

	/// <summary>The effective FullHealth of a token. Minimum of 1; </summary>
	public int FullHealth => Math.Max(1, _rawFullHealth - StrifeCount * HealthPenaltyPerStrife );
	readonly int _rawFullHealth; // the value adjusted by modifications, may be less than 1.

	public int Damage { get; }

	public int StrifeCount { get; }

	public bool IsDestroyed => FullHealth <= Damage;

	#region Token mutation generators

	public HealthToken HavingStrife(int strifeCount) {
		return strifeCount <0 ? throw new System.ArgumentOutOfRangeException(nameof(strifeCount),$"strife Count = {strifeCount}")
			: strifeCount == StrifeCount ? this
			: new HealthToken( Class, _rawFullHealth, Damage, strifeCount );
	}

	public HealthToken AddStrife( int deltaStrife ) => HavingStrife( StrifeCount + deltaStrife );

	public HealthToken AddDamage( int damage ) => new HealthToken( Class, _rawFullHealth, Math.Min( Damage + damage, _rawFullHealth ), StrifeCount ); // ??? Is this Math.Min necessary, could we just let it go negative?

	public HealthToken Healthy => new HealthToken( Class, FullHealth, 0, StrifeCount ); // no damage

	public HealthToken AddHealth( int delta ) {
		int newHealth = Math.Max(1, _rawFullHealth + delta );
		return newHealth == _rawFullHealth ? this
			: new HealthToken( Class, newHealth, Damage, StrifeCount );
	}

	#endregion

	#region GetHashCode / Equals

	public override int GetHashCode() 
		=> Class.GetHashCode()
		+  2 * _rawFullHealth	// Do NOT use FullHealth because that might change based on HealthPenalty
		+  8 * Damage
		+ 32 * StrifeCount;

	public override bool Equals( object obj ) => this.Equals( obj as HealthToken );

	public bool Equals( HealthToken other ) {
		return other != null
			&& other.Class == Class
			&& other._rawFullHealth == _rawFullHealth
			&& other.Damage == Damage	
			&& other.StrifeCount == StrifeCount;
	}

	#endregion

	public int RemainingHealth => FullHealth - Damage;

	public override string ToString() => _summaryString;

	readonly string _summaryString;

	string IOption.Text => _summaryString;

}