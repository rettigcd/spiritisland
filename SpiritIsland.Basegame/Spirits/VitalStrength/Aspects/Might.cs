namespace SpiritIsland.Basegame;

public class Might : IAspect {

	// https://spiritislandwiki.com/index.php?title=Might

	static public AspectConfigKey ConfigKey => new AspectConfigKey(VitalStrength.Name, Name);
	public const string Name = "Might";
	public string[] Replaces => [EarthsVitality.Name];

	public void ModSpirit(Spirit spirit) {
		EarthsVitality.ReplaceWith(spirit,new SpiritPresenceToken(spirit));
		EarthMovesWithVigorAndMight.InitAspect(spirit);
		spirit.SpecialRules = [];
	}

}