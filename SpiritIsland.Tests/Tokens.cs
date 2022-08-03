
namespace SpiritIsland.Tests;

internal class Tokens {

	// Standard Invader values - Used in tests - easier than trying to pull from the game.
	public class X : IHaveHealthPenaltyPerStrife {
		public int HealthPenaltyPerStrife { get; set; }
	}

	// Instead of tokens being STATIC, make instance to de-couple the penaltyHolder

	static Tokens() {
		_penaltyHolder = new X();
		City = new HealthToken( Invader.City, _penaltyHolder, 3, 0 );
		City2 = new HealthToken( Invader.City, _penaltyHolder, 3, 1 );
		City1 = new HealthToken( Invader.City, _penaltyHolder, 3, 2 );
		Town = new HealthToken( Invader.Town, _penaltyHolder, 2, 0 );
		Town1 = new HealthToken( Invader.Town, _penaltyHolder, 2, 1 );
		Explorer = new HealthToken( Invader.Explorer, _penaltyHolder, 1, 0 );
		Dahan = new HealthToken( TokenType.Dahan, _penaltyHolder, 2, 0 );
		Dahan1 = new HealthToken( TokenType.Dahan, _penaltyHolder, 2, 1 );
	}

	static public readonly X _penaltyHolder;

	static public readonly HealthToken City;
	static public readonly HealthToken City2;
	static public readonly HealthToken City1;

	static public readonly HealthToken Town;
	static public readonly HealthToken Town1;

	static public readonly HealthToken Explorer;

	static public readonly HealthToken Dahan;
	static public readonly HealthToken Dahan1;

}
