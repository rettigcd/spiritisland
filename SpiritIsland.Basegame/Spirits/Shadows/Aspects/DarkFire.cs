

namespace SpiritIsland.Basegame;

public class DarkFire : IAspect {

	// https://spiritislandwiki.com/index.php?title=Dark_Fire

	// Setup Gain Unquenchable Flames(Minor Power)

	static public AspectConfigKey ConfigKey => new AspectConfigKey(Shadows.Name, Name);
	public const string Name = "Dark Fire";
	public string[] Replaces => [ShadowsOfTheDahan.Name];

	public void ModSpirit(Spirit spirit) {
		// Remove
		ShadowsOfTheDahan.RemoveFrom(spirit);

		// Add
		DarkAndFireAsOne.InitAspect(spirit);
		FrightfulShadowsEludeDestruction.InitAspect(spirit);
		AddBonusElementToPresenceTrack(spirit, Track.FireEnergy);

		spirit.SpecialRules = [DarkAndFireAsOne.Rule, FrightfulShadowsEludeDestruction.Rule];
	}

	static void AddBonusElementToPresenceTrack(Spirit spirit,Track src) {
		Track t = spirit.Presence.Energy.Slots.First();
		// assuming Energy is 0...
		t.Code = src.Code; 
		t.Elements = src.Elements; 
		t.Icon = src.Icon;
	}
}
