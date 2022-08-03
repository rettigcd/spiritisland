
namespace SpiritIsland.Tests;

internal static class StdTokens {

	class X : IHaveHealthPenaltyPerStrife {
		public int HealthPenaltyPerStrife => 0;
	}

	static StdTokens() {
		var _penaltyHolder = new X();
		City = new HealthToken( Invader.City, _penaltyHolder, 3, 0 );
		City2 = new HealthToken( Invader.City, _penaltyHolder, 3, 1 );
		City1 = new HealthToken( Invader.City, _penaltyHolder, 3, 2 );
		Town = new HealthToken( Invader.Town, _penaltyHolder, 2, 0 );
		Town1 = new HealthToken( Invader.Town, _penaltyHolder, 2, 1 );
		Explorer = new HealthToken( Invader.Explorer, _penaltyHolder, 1, 0 );
		Dahan = new HealthToken( TokenType.Dahan, _penaltyHolder, 2, 0 );
		Dahan1 = new HealthToken( TokenType.Dahan, _penaltyHolder, 2, 1 );
	}

	public static readonly HealthToken City;
	public static readonly HealthToken City2;
	public static readonly HealthToken City1;

	public static readonly HealthToken Town;
	public static readonly HealthToken Town1;

	public static readonly HealthToken Explorer;

	public static readonly HealthToken Dahan;
	public static readonly HealthToken Dahan1;

}
