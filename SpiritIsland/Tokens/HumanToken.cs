namespace SpiritIsland;

/// <summary>
/// Base token for both Dahan and Invaders
/// </summary>
public class HumanToken : IToken, IAppearInSpaceAbreviation, IEquatable<HumanToken> {

	class NoPenalty : IHaveHealthPenaltyPerStrife { public int HealthPenaltyPerStrife => 0; }

	public HumanToken( HumanTokenClass tokenClass, int rawFullHealth ) {
		HumanClass = tokenClass;
		_rawFullHealth = rawFullHealth;
		Damage = 0;
		DreamDamage = 0;
		StrifeCount = 0;
		IHaveHealthPenaltyPerStrife penalty = GameState.Current as IHaveHealthPenaltyPerStrife;
		_healthPenaltyHolder = penalty ?? new NoPenalty();// penaltyHolder;
		_summaryString = HumanClass.Initial + "@" + RemainingHealth;
		Attack = rawFullHealth;

		bool isDahan = tokenClass == Human.Dahan;
		RavageOrder = isDahan ? RavageOrder.DahanTurn : RavageOrder.InvaderTurn;
		RavageSide = isDahan ? RavageSide.Defender : RavageSide.Attacker;
	}

	protected HumanToken( Props x ) {
		HumanClass = x.Class;
		_rawFullHealth = x._rawFullHealth;

		RavageOrder = x.RavageOrder;
		RavageSide = x.RavageSide;

		Damage = x.Damage;
		DreamDamage = x.DreamDamage;
		StrifeCount = x.StrifeCount;

		_healthPenaltyHolder = (IHaveHealthPenaltyPerStrife)GameState.Current ?? new NoPenalty(); // x._helathPenaltyHolder;

		_summaryString = HumanClass.Initial + "@" + RemainingHealth
			+ (x.DreamDamage == 0 ? "" : new string( '~', DreamDamage ))
			+ (x.StrifeCount == 0 ? "" : new string( '^', StrifeCount ));

		Attack = x.Attack;
	}

	public HumanTokenClass HumanClass { get; }
	public ITokenClass Class => HumanClass;

	public bool HasTag(ITag tag) => HumanClass.HasTag( tag );

	/// <summary>The effective FullHealth of a token. Minimum of 1; </summary>
	public int FullHealth => Math.Max(1, _rawFullHealth - StrifeCount * _healthPenaltyHolder.HealthPenaltyPerStrife );
	readonly int _rawFullHealth; // the value adjusted by modifications, may be less than 1.

	public int Damage { get; }

	public int Attack { get; }

	public int DreamDamage { get; }

	public RavageOrder RavageOrder { get; }
	public RavageSide RavageSide {get;}
	
	public int FullDamage => Damage + DreamDamage;

	public int StrifeCount { get; }

	public bool IsDestroyed => FullHealth <= FullDamage;

	#region Token mutation generators

	/// <returns>a new token with the adjusted strife</returns>
	public HumanToken AddStrife( int deltaStrife ) => HavingStrife( StrifeCount + deltaStrife );

	public HumanToken HavingStrife( int strifeCount ) {
		return strifeCount < 0 ? throw new System.ArgumentOutOfRangeException( nameof( strifeCount ), $"strife Count = {strifeCount}" )
			: strifeCount == StrifeCount ? this
			: Mutate( x => x.StrifeCount = strifeCount );
	}

	public HumanToken AddDamage( int damage, int nightmareDamage=0 ) {
		int newDamage = Math.Min( Damage + damage, _rawFullHealth ); // Give regular damage priority
		// only allow nightmare damage to take up whatever remaining health is available
		int newNightmareDamage = Math.Min( nightmareDamage + DreamDamage, _rawFullHealth-newDamage );
		return Mutate( x=>{ x.Damage=newDamage; x.DreamDamage = newNightmareDamage; } );
	}

	public HumanToken SetAttack( int attack ) => Mutate( x=>x.Attack = attack );
	public HumanToken SetRavageOrder( RavageOrder round) => Mutate(x=>x.RavageOrder = round );
	public HumanToken SetRavageSide( RavageSide side ) => Mutate( x => x.RavageSide = side );

	// For Dream a 1000 Deaths, make token not in the Invader TokenCategory
	public HumanToken SwitchClass( HumanTokenClass newClass ) => Mutate( x=>x.Class = newClass );

	public HumanToken Healthy => Mutate( x=> { x.Damage=0; x.DreamDamage=0; } );

	public HumanToken AddHealth( int delta ) {
		int newHealth = Math.Max(1, _rawFullHealth + delta );
		return newHealth == _rawFullHealth ? this
			: Mutate( x => x._rawFullHealth = newHealth );
	}

	/// <summary> Used by mutation generators to create a new token of the same class. </summary>
	HumanToken Mutate(Action<Props> mod) {
		Props x = GetProps();
		mod( x );
		return MakeNew( x );
	}
	public class Props {
		public HumanTokenClass Class;
		public int _rawFullHealth;
		public int Damage;
		public int StrifeCount;
		public int DreamDamage;
		public int Attack;
		public RavageOrder RavageOrder;
		public RavageSide RavageSide;
	}

	protected virtual HumanToken MakeNew( Props x ) => new HumanToken( x );

	public Props GetProps() {
		return new Props {
			Class = HumanClass,
			_rawFullHealth = _rawFullHealth,
			Damage = Damage,
			StrifeCount = StrifeCount,
			DreamDamage = DreamDamage,
			Attack = Attack,
			RavageOrder = RavageOrder,
			RavageSide = RavageSide
		};
	}

	#endregion

	#region GetHashCode / Equals

	public override int GetHashCode() 
		=> HumanClass.GetHashCode()
		// not using 2,3,5 because those are all valid values.
		+ 7 * _rawFullHealth	// Do NOT use FullHealth because that might change based on HealthPenalty
		+ 11 * Damage
		+ 13 * DreamDamage
		+ 17 * StrifeCount
		+ 19 * Attack
		+ 23 * (int)RavageOrder
		+ 29 * (int)RavageSide;

	public override bool Equals( object obj ) => this.Equals( obj as HumanToken );

	public bool Equals( HumanToken other ) {
		return other != null
			&& other.HumanClass == HumanClass
			&& other._rawFullHealth == _rawFullHealth
			&& other.Damage == Damage	
			&& other.StrifeCount == StrifeCount
			&& other.Attack == Attack
			&& other.RavageOrder == RavageOrder
			&& other.RavageSide == RavageSide;
	}

	#endregion

	public int RemainingHealth => FullHealth - FullDamage;

	public override string ToString() => _summaryString;

	string IOption.Text => _summaryString;

	public string SpaceAbreviation => _summaryString;

	public Img Img => HumanClass.Img;

	readonly string _summaryString;

	readonly public IHaveHealthPenaltyPerStrife _healthPenaltyHolder;
}
