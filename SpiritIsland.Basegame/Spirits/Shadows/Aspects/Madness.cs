

namespace SpiritIsland.Basegame;

public class Madness : IAspect {

	// https://spiritislandwiki.com/index.php?title=Madness

	static public AspectConfigKey ConfigKey => new AspectConfigKey(Shadows.Name, Name);
	public const string Name = "Madness";
	public string[] Replaces => [ShadowsOfTheDahan.Name];

	public void ModSpirit(Spirit spirit) {
		ShadowsOfTheDahan.RemoveFrom(spirit);

		// Rule 1
		ShadowsCastASubtleMadness_Init(spirit);

		// Rule 2
		var old = spirit.Presence;
		spirit.Presence = new SpiritPresence(spirit, old.Energy, old.CardPlays, new GlimpsOfTheShadowedVoid(spirit));
		// !!! Maybe it would be better to add an OnDestroyed event handler so we didn't have to swap the whole thing out.

		spirit.SpecialRules = [SubtleMadnessRule, GlimpsOfTheShadowedVoid.Rule];
	}

	static void ShadowsCastASubtleMadness_Init(Spirit spirit) {
		foreach( var pp in spirit.GrowthTrack.GrowthActions.OfType<PlacePresence>() )
			pp.Placed.Add(args => spirit.Target((Space)args.To).AddStrife(1));
	}

	static SpecialRule SubtleMadnessRule => new SpecialRule(
		"Shadows Cast a Subtle Madness",
		"When you add Presence during Growth, you may also add 1 Strife in that land."
	);

}

class GlimpsOfTheShadowedVoid(Spirit spirit) : SpiritPresenceToken(spirit), IHandleTokenRemoved {

	public const string Name = "Glimpse of the Shadowed Void";
	const string Description = "When your Presence is Destroyed, if Invaders are present, 1 Fear per Presence Destroyed there.";
	static public SpecialRule Rule => new SpecialRule(Name,Description);

	protected override Task OnPresenceDestroyed(ITokenRemovedArgs args) {
		return args.Reason == RemoveReason.Destroyed
			&& args.Removed == this
			&& args.From is Space space
			&& space.HasInvaders()
			? space.AddFear(args.Count)
			: Task.CompletedTask;
	}

}