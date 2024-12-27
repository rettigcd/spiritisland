using SpiritIsland.Invaders.Ravage;

namespace SpiritIsland;

/// <summary>
/// Base token for both Dahan and Invaders
/// </summary>
public class HumanToken : IToken, IAppearInSpaceAbreviation, IEquatable<HumanToken> {

	#region constructors

	/// <summary>
	/// Constructs a Human Token with the expected Health from the HumanTokenClass.
	/// </summary>
	public HumanToken( HumanTokenClass tokenClass ):this(tokenClass,tokenClass.ExpectedHealthHint) {}

	/// <summary>
	/// Specifies the Health of this token
	/// </summary>
	public HumanToken( HumanTokenClass tokenClass, int rawFullHealth ) {
		HumanClass = tokenClass;
		Img = tokenClass.Img;
		_rawFullHealth = rawFullHealth;
		Damage = 0;
		StrifeCount = 0;
		_summaryString = HumanClass.Initial + "@" + RemainingHealth;
		Attack = rawFullHealth;

		bool isDahan = tokenClass == Human.Dahan;
		RavageOrder = isDahan ? RavageOrder.DahanTurn : RavageOrder.InvaderTurn;
		RavageSide = isDahan ? RavageSide.Defender : RavageSide.Attacker;
	}

	protected HumanToken(Props x) {
		HumanClass = x.Class;
		Img = x.Img;
		_rawFullHealth = x._rawFullHealth;

		RavageOrder = x.RavageOrder;
		RavageSide = x.RavageSide;

		Damage = x.Damage;
		StrifeCount = x.StrifeCount;

		_summaryString = HumanClass.Initial + "@" + RemainingHealth 
			+ (x.StrifeCount == 0 ? "" : new string('^', StrifeCount));

		Attack = x.Attack;
	}

	#endregion constructors

	public string Badge => HasInterestingHealthValue ? RemainingHealth.ToString() : string.Empty;

	bool HasInterestingHealthValue => 0 < Damage  // has damage
		|| FullHealth != HumanClass.ExpectedHealthHint; // has unusuall full health value

	public HumanTokenClass HumanClass { get; }
	public ITokenClass Class => HumanClass;

	public Img Img { get; }

	public bool HasTag(ITag tag) => HumanClass.HasTag( tag );

	/// <summary>The effective FullHealth of a token. Minimum of 1; </summary>
	public int FullHealth => Math.Max(1, _rawFullHealth );
	readonly int _rawFullHealth; // the value adjusted by modifications, may be less than 1.

	public int Damage { get; }

	public int Attack { get; }

	public RavageOrder RavageOrder { get; }
	public RavageSide RavageSide {get;}

	public int StrifeCount { get; }

	public bool IsDestroyed => FullHealth <= Damage;

	#region Token mutation generators

	/// <returns>a new token with the adjusted strife</returns>
	public HumanToken AddStrife( int deltaStrife ) => HavingStrife( StrifeCount + deltaStrife );

	public HumanToken HavingStrife( int strifeCount ) {
		return strifeCount < 0 ? throw new System.ArgumentOutOfRangeException( nameof( strifeCount ), $"strife Count = {strifeCount}" )
			: strifeCount == StrifeCount ? this
			: Mutate( x => x.StrifeCount = strifeCount );
	}

	public HumanToken AddDamage( int damage ) {
		int newDamage = Math.Min( Damage + damage, _rawFullHealth ); // Give regular damage priority
		return Mutate( x=>{ x.Damage=newDamage; } );
	}

	public HumanToken SetAttack( int attack ) => Mutate( x=>x.Attack = attack );
	public HumanToken SetRavageOrder( RavageOrder round) => Mutate(x=>x.RavageOrder = round );
	public HumanToken SetRavageSide( RavageSide side ) => Mutate( x => x.RavageSide = side );
	public HumanToken ChangeImg( Img newImg ) => Mutate( x => x.Img = newImg );

	public HumanToken Healthy => Mutate( x=> { x.Damage=0; } );

	public HumanToken AddHealth( int delta ) {
		int newHealth = Math.Max(1, _rawFullHealth + delta );
		return newHealth == _rawFullHealth ? this
			: Mutate( x => x._rawFullHealth = newHealth );
	}

	/// <summary> Used by mutation generators to create a new token of the same class. </summary>
	HumanToken Mutate(Action<Props> mod) {
		Props x = GetProps();
		mod( x );
		return MakeNew(x);
	}

	// virtual so HabsburgDurableToken can return a HabsburgDurableToken instead of a HumanToken
	protected virtual HumanToken MakeNew(Props x) => new HumanToken(x);

	public class Props {
		public required HumanTokenClass Class;
		public int _rawFullHealth;
		public int Damage;
		public int StrifeCount;
		public int Attack;
		public RavageOrder RavageOrder;
		public RavageSide RavageSide;
		public Img Img;
	}

	public Props GetProps() {
		return new Props {
			Class = HumanClass,
			_rawFullHealth = _rawFullHealth,
			Damage = Damage,
			StrifeCount = StrifeCount,
			Attack = Attack,
			RavageOrder = RavageOrder,
			RavageSide = RavageSide,
			Img = Img
		};
	}

	#endregion

	#region GetHashCode / Equals

	public override int GetHashCode() 
		=> HumanClass.GetHashCode()
		// not using 2,3,5 because those are all valid values.
		+ 7 * _rawFullHealth	// Do NOT use FullHealth because that might change based on HealthPenalty
		+ 11 * Damage
		+ 13 * StrifeCount
		+ 17 * Attack
		+ 19 * (int)RavageOrder
		+ 23 * (int)RavageSide
		+ 29 * (int)Img;

	public override bool Equals( object? obj ) => this.Equals( obj as HumanToken );

	public bool Equals( HumanToken? other ) {
		return other is not null
			&& other.HumanClass == HumanClass
			&& other._rawFullHealth == _rawFullHealth
			&& other.Damage == Damage	
			&& other.StrifeCount == StrifeCount
			&& other.Attack == Attack
			&& other.RavageOrder == RavageOrder
			&& other.RavageSide == RavageSide
			&& other.Img == Img;
	}

	#endregion

	/// <summary> FullHealth-Damage </summary>
	public int RemainingHealth => FullHealth - Damage;

	public override string ToString() => _summaryString;

	string IOption.Text => _summaryString;

	public string SpaceAbreviation => _summaryString;

	readonly string _summaryString;

}
