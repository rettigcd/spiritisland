namespace SpiritIsland;

public static class Target {
	public const string Any              = "Any";

	public const string Jungle           = "Jungle";
	public const string Sand             = "Sand";
	public const string Mountain         = "Mountain";
	public const string NotWetland       = "Not Wetland";
	public const string Wetland          = "Wetland";
	public const string Coastal          = "coastal";
	public const string Ocean            = "Ocean";
	public const string NotOcean         = "not ocean";
	public const string Inland           = "Inland";

	public const string Blight           = "Blight";
	public const string NoBlight         = "no blight";

	public const string Dahan            = "Dahan";

	public const string Invaders         = "Invaders";
	public const string NoInvader        = "no invader";
	public const string ExplorerOrTown   = "TownOrExplorer";
	public const string TownOrCity       = "TownOrCity";
	public const string City             = "Cities";

	public const string Disease          = "Disease";
	public const string Wilds            = "Wilds";
	public const string Beast            = "Beast";
	public const string TwoBeasts        = "2 Beasts";

	public const string Presence         = "Presence";
	public const string BlightAndInvaders= "Blight+Invaders";
}

static public class FilterEnumExtension {
	static public Img GetImgEnum( string filterEnum ) {
		// !!! Move the filterEnum closer to where the filter is defined, not here.
		Img img = filterEnum switch {
			Target.Dahan                         => Img.Icon_Dahan,
			Target.Jungle+"Or"+Target.Wetland    => Img.Icon_JungleOrWetland,
			Target.Dahan+"Or"+Target.Invaders    => Img.Icon_DahanOrInvaders,
			Target.Coastal                       => Img.Icon_Coastal,
			Target.Presence+"Or"+Target.Wilds    => Img.Icon_PresenceOrWilds,
			Target.NoBlight                      => Img.Icon_NoBlight,
			Target.Beast+"Or"+Target.Jungle      => Img.Icon_BeastOrJungle,
			Target.Ocean                         => Img.Icon_Ocean,
			Target.Mountain+"Or"+Target.Presence => Img.Icon_MountainOrPresence,
			Target.TownOrCity+"Or"+Target.Blight => Img.Icon_TownCityOrBlight,
			_                                    => Img.None, // Inland, Any
		};
		return img;
	}

}