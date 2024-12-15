namespace SpiritIsland.Basegame;

public class AnchorTheLandsResilience(Spirit spirit) : SpiritPresenceToken(spirit), IModifyBlightThreshold {

	public const string Name = "Anchor the Land's Resilience";
	const string Description = "In lands with your Sacred Site, it takes 8 additional Damage to add Blight to the land.";
	static public SpecialRule Rule => new SpecialRule(Name, Description);

	void IModifyBlightThreshold.ModifyLandsResilience(Space space,ref int blightThreshold) {
		if(Self.Presence.IsSacredSite(space))
			blightThreshold += 8;
	}
} 