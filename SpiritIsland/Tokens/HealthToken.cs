namespace SpiritIsland;

public class HealthToken : Token, IEquatable<HealthToken> {


	public HealthToken( HealthTokenClass tokenClass, int fullHealth, int damage = 0, int strifeCount = 0 ) {
		Class = tokenClass;
		_fullHealth = fullHealth;
		Damage = damage;
		StrifeCount = strifeCount;

		_summaryString = Initial + "@" + RemainingHealth
			+ (strifeCount == 0 ? "" : new string( '^', StrifeCount ));
	}

	public HealthTokenClass Class { get; }
	TokenClass Token.Class => this.Class;

	static public int HealthPenaltyPerStrife { get; set; } = 0; // !!! put this in the game state instead of static global

	public int FullHealth => Math.Max(1, _fullHealth - StrifeCount * HealthPenaltyPerStrife );
	readonly int _fullHealth;

	public int Damage { get; }

	public int StrifeCount { get; }

	public bool IsDestroyed => FullHealth <= Damage;

	#region Token mutation generators

	public HealthToken HavingStrife(int strifeCount) {
		return strifeCount <0 ? throw new System.ArgumentOutOfRangeException(nameof(strifeCount),$"strife Count = {strifeCount}")
			: strifeCount == StrifeCount ? this
			: new HealthToken( Class, FullHealth, Damage, strifeCount );
	}

	public HealthToken AddStrife( int deltaStrife ) => HavingStrife( StrifeCount + deltaStrife );

	public HealthToken AddDamage( int damage ) => new HealthToken( Class, FullHealth, Math.Min( Damage + damage, FullHealth), StrifeCount );

	public HealthToken Healthy => new HealthToken( Class, FullHealth, 0, StrifeCount );

	/// <summary>
	/// Adjusts Health to a minimum of 1.
	/// </summary>
	public HealthToken AddHealth( int delta ) {
		int newHealth = Math.Max(1, FullHealth + delta );
		return newHealth == FullHealth ? this
			: new HealthToken( Class, newHealth, Damage, StrifeCount );
	}

	#endregion

	#region GetHashCode / Equals

	public override int GetHashCode() => Class.GetHashCode()*7 + FullHealth + Damage * 100 + StrifeCount * 10000;

	public override bool Equals( object obj ) => this.Equals( obj as HealthToken );
	public bool Equals( HealthToken other ) {
		return other != null
			&& other.Class == Class
			&& other.FullHealth == FullHealth
			&& other.Damage == Damage	
			&& other.StrifeCount == StrifeCount;
	}

	#endregion

	public int RemainingHealth => FullHealth - Damage;

	public override string ToString() => _summaryString;
	readonly string _summaryString;

	public char Initial => Class.Initial;

	string IOption.Text => _summaryString;

}