

namespace SpiritIsland.Basegame;

class GlimpsOfTheShadowedVoid(Spirit spirit) : SpiritPresenceToken(spirit) {

	public const string Name = "Glimpse of the Shadowed Void";
	const string Description = "When your Presence is Destroyed, if Invaders are present, 1 Fear per Presence Destroyed there.";
	static public SpecialRule Rule => new SpecialRule(Name,Description);

	static public void InitAspect(Spirit spirit) {
		var old = spirit.Presence;
		spirit.Presence = new SpiritPresence(spirit, old.Energy, old.CardPlays, new GlimpsOfTheShadowedVoid(spirit));
		// !!! Maybe it would be better to add an OnDestroyed event handler so we didn't have to swap the whole thing out.
	}

	protected override Task OnPresenceDestroyed(ITokenRemovedArgs args) {
		return args.Reason == RemoveReason.Destroyed
			&& args.Removed == this
			&& args.From is Space space
			&& space.HasInvaders()
			? space.AddFear(args.Count)
			: Task.CompletedTask;
	}

}