﻿namespace SpiritIsland;

public static class Target {
	public const string Any              = "Any";
	public const string Dahan            = "dahan";
	public const string Disease          = "disease";
	public const string Invaders         = "Invaders";
	public const string Jungle           = "J";
	public const string JungleOrMountain = "J / M";
	public const string JungleOrSand     = "J / S";
	public const string JungleOrWetland  = "W / J";
	public const string JungleWithNoBlight = "J / no blight";
	public const string Mountain         = "Mountain";
	public const string MountainOrWetland= "MountainOrWetland";
	public const string MountainOrSand   = "MountainOrSand";
	public const string SandOrWetland    = "SandOrWetland";
	public const string NotWetland       = "Not Wetland";
	public const string Wetland          = "Wetland";
	public const string Blight           = "Blight";
	public const string TownCityOrBlight = "TownCityOrBlight";
	public const string NoBlight         = "no blight";
	public const string NoInvader        = "no invader";
	public const string DahanOrInvaders  = "dahan or invaders";
	public const string Coastal          = "coastal";
	public const string Ocean            = "Ocean";
	public const string NotOcean         = "not ocean";

	public const string TownOrExplorer   = "town or explorer"; // wash away helper
	public const string TownOrCity       = "town or city"; // Study the Invaders' Fears


	// BAC
	public const string BeastOrJungle    = "beast or jungle";
	public const string Beast            = "Beast";
	public const string TwoBeasts        = "2 Beasts";
	public const string PresenceOrWilds  = "presence or wilds";
	public const string Inland           = "Inland";
	public const string CoastalOrWetlands= "CostalOrWetlands";
	public const string City             = "Cities";

	// JE
	public const string MountainOrPresence = "mountain or presence";
}

static public class FilterEnumExtension {
	static public Img GetImgEnum( string filterEnum ) {
		// !!! Move the filterEnum closer to where the filter is defined, not here.
		Img img = filterEnum switch {
			Target.Dahan => Img.Icon_Dahan,
			Target.JungleOrWetland => Img.Icon_JungleOrWetland,
			Target.DahanOrInvaders => Img.Icon_DahanOrInvaders,
			Target.Coastal => Img.Icon_Coastal,
			Target.PresenceOrWilds => Img.Icon_PresenceOrWilds,
			Target.NoBlight => Img.Icon_NoBlight,
			Target.BeastOrJungle => Img.Icon_BeastOrJungle,
			Target.Ocean => Img.Icon_Ocean,
			Target.MountainOrPresence => Img.Icon_MountainOrPresence,
			Target.TownCityOrBlight => Img.Icon_TownCityOrBlight,
			_ => Img.None, // Inland, Any
		};
		return img;
	}

}