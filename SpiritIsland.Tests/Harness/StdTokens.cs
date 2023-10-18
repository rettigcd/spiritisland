namespace SpiritIsland.Tests;

internal static class StdTokens {

	static StdTokens() {
		City = new HumanToken( Human.City, 3 );
		City2 = City.AddDamage(1);
		City1 = City2.AddDamage(1);
		Town = new HumanToken( Human.Town, 2 );
		Town1 = Town.AddDamage(1);
		Explorer = new HumanToken( Human.Explorer, 1 );
		Dahan = new HumanToken( Human.Dahan, 2 );
		Dahan1 = Dahan.AddDamage(1);
		Disease = (TokenClassToken)Token.Disease;
	}

	public static readonly HumanToken City;
	public static readonly HumanToken City2;
	public static readonly HumanToken City1;

	public static readonly HumanToken Town;
	public static readonly HumanToken Town1;

	public static readonly HumanToken Explorer;

	public static readonly HumanToken Dahan;
	public static readonly HumanToken Dahan1;
	public static readonly TokenClassToken Disease;

}
