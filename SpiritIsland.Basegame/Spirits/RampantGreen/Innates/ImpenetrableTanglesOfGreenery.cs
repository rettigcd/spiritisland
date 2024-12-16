
namespace SpiritIsland.Basegame.Spirits.RampantGreen.Aspects;

[InnatePower(Name), Fast]
[FromPresence(0)]
public class ImpenetrableTanglesOfGreenery {

	public const string Name = "Impenetrable Tangles of Greenery";

	[InnateTier("1 sun,2 plant", "This Power may target any terrain.")]
	static public Task AddWilds(TargetSpaceCtx ctx) {
		// Add 1 Wilds.
		return ctx.Wilds.AddAsync(1);
	}

	[InnateTier("2 sun,3 plant", "Push 1 Explorer.",1)]
	static public Task PushExplorer(TargetSpaceCtx ctx) {
		// Push 1 Explorer.
		return ctx.Push(1, Human.Explorer);
	}

	[InnateTier("2 sun,4 plant", "Isolate target land.", 2)]
	static public Task Add2DestroyedPresenceAsync(TargetSpaceCtx ctx) {
		// Isolate target land.
		ctx.Isolate();
		return Task.CompletedTask;
	}

	[InnateTier("3 sun,5 plant", "Downgrade 1 City. Add 1 Wilds.", 3)]
	static public async Task DowngradeCityAndAddWildsAsync(TargetSpaceCtx ctx) {
		// Downgrade 1 City. Add 1 Wilds.
		await ReplaceInvader.Downgrade1(ctx.Self,ctx.Space,Present.Always, Human.City);
		await ctx.Wilds.AddAsync(1);
	}

}