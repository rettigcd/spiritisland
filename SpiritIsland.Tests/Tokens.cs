
namespace SpiritIsland.Tests;

internal class Tokens {

	// Standard Invader values - Used in tests - easier than trying to pull from the game.

	static public readonly HealthToken City = new HealthToken( Invader.City, 3, 0 );
	static public readonly HealthToken City2= new HealthToken( Invader.City, 3, 1 );
	static public readonly HealthToken City1= new HealthToken( Invader.City, 3, 2 );

	static public readonly HealthToken Town = new HealthToken( Invader.Town, 2, 0 );
	static public readonly HealthToken Town1 = new HealthToken( Invader.Town, 2, 1 );

	static public readonly HealthToken Explorer = new HealthToken( Invader.Explorer, 1, 0 );

	static public readonly HealthToken Dahan = new HealthToken( TokenType.Dahan, 2, 0 );
	static public readonly HealthToken Dahan1 = new HealthToken( TokenType.Dahan, 2, 1 );

}

