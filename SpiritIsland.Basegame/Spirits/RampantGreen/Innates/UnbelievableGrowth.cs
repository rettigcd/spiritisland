
namespace SpiritIsland.Basegame.Spirits.RampantGreen.Aspects;

[InnatePower(Name), Slow]
[FromPresenceThresholdAlternate(1, [Filter.Jungle, Filter.Wetland], "2 water,3 plant", 1, [Filter.Any])]
public class UnbelievableGrowth {

	public const string Name = "Unbelievable Growth";

	[InnateTier("2 water,3 plant", "This Power may target any terrain.")]
	static public Task TargetAnyLand(TargetSpaceCtx _) {
		// This Power may target any terrain.
		return Task.CompletedTask;
	}

	[InnateTier("1 water,3 plant", Add1DestroyedPresence)]
	static public Task Add1DestroyedPresenceAsync(TargetSpaceCtx ctx) {
		return ctx.Self.Presence.Destroyed.MoveToAsync(ctx.Space, 1);
	}

	[InnateTier("2 water,4 plant", Add1DestroyedPresence, 2)]
	static public Task Add2DestroyedPresenceAsync(TargetSpaceCtx ctx) {
		return ctx.Self.Presence.Destroyed.MoveToAsync(ctx.Space, 2);
	}

	[InnateTier("3 water,5 plant", Add1DestroyedPresence, 3)]
	static public Task Add3DestroyedPresenceAsync(TargetSpaceCtx ctx) {
		return ctx.Self.Presence.Destroyed.MoveToAsync(ctx.Space, 3);
	}

	const string Add1DestroyedPresence = "Add 1 Destroyed Presence.";

}
