namespace SpiritIsland;

public class HealthToken : Token, IEquatable<HealthToken> {


	public HealthToken( HealthTokenClass tokenClass, int fullHealth, int damage = 0, int strifeCount = 0 ) {
		Class = tokenClass;
		FullHealth = fullHealth;
		Damage = damage;
		StrifeCount = strifeCount;

		Summary = Initial + "@" + RemainingHealth
			+ (strifeCount == 0 ? "" : new string( '^', StrifeCount ));
	}

	public HealthTokenClass Class { get; }
	TokenClass Token.Class => this.Class;

	public int FullHealth { get; }

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

	public HealthToken AddHealth( int healthBoost ) => new HealthToken( Class, FullHealth + healthBoost, Damage, StrifeCount );

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

	public virtual string Summary { get; }

	public override string ToString() => Summary; // for showing keys of CountDictionary<Token>

	public char Initial => Class.Initial;

	string IOption.Text =>  Summary;

}