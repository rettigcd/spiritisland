namespace SpiritIsland;

public static class Target {
	public const string Any        = "Any";

	public const string Jungle     = "Jungle";
	public const string Sand       = "Sand";
	public const string Mountain   = "Mountain";
	public const string Wetland    = "Wetland";
	public const string Coastal    = "coastal";
	public const string Ocean      = "Ocean";
	public const string Inland     = "Inland";

	public const string Blight     = "Blight";
	public const string Dahan      = "Dahan";

	public const string Invaders   = "Invaders";
	public const string City       = "Cities";
	public const string Town       = "Town";

	public const string Disease    = "Disease";
	public const string Wilds      = "Wilds";
	public const string Beast      = "Beast";
	public const string Presence   = "Presence";

	public const string NotWetland = "Not " + Wetland;
	public const string NotOcean   = "Not " + Ocean;
	public const string NoBlight   = "No " + Blight;
	public const string NoInvader  = "No " + Invaders;

	public const string TwoBeasts        = "2 " + Beast;
	public const string BlightAndInvaders= Blight + "+" + Invaders;
	public const string TwoBeastPlusInvaders = TwoBeasts + "+" + Invaders;
}