namespace SpiritIsland.Tests;

internal static class StdTokens {

	class X : IHaveHealthPenaltyPerStrife {
		public int HealthPenaltyPerStrife => 0;
	}

	static StdTokens() {
		var _penaltyHolder = new X();
		City = new HumanToken( Human.City, _penaltyHolder, 3, 0 );
		City2 = new HumanToken( Human.City, _penaltyHolder, 3, 1 );
		City1 = new HumanToken( Human.City, _penaltyHolder, 3, 2 );
		Town = new HumanToken( Human.Town, _penaltyHolder, 2, 0 );
		Town1 = new HumanToken( Human.Town, _penaltyHolder, 2, 1 );
		Explorer = new HumanToken( Human.Explorer, _penaltyHolder, 1, 0 );
		Dahan = new HumanToken( Human.Dahan, _penaltyHolder, 2, 0 );
		Dahan1 = new HumanToken( Human.Dahan, _penaltyHolder, 2, 1 );
	}

	public static readonly HumanToken City;
	public static readonly HumanToken City2;
	public static readonly HumanToken City1;

	public static readonly HumanToken Town;
	public static readonly HumanToken Town1;

	public static readonly HumanToken Explorer;

	public static readonly HumanToken Dahan;
	public static readonly HumanToken Dahan1;

}
