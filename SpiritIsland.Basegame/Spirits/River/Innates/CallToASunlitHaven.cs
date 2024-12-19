namespace SpiritIsland.Basegame;

[InnatePower(Name,Instructions), Fast,FromSacredSite(0)]
public class CallToASunlitHaven {

	public const string Name = "Call to a Sunlit Haven";
	const string Instructions = "E is the highest unconvered number on your Energy track.";

	static public void InitAspect(Spirit spirit) => spirit.InnatePowers[0] = InnatePower.For(typeof(CallToASunlitHaven));

	[InnateTier("1 sun,1 water", "Defend E.")]
	static public Task DefendE(TargetSpaceCtx ctx) {
		int e = GetE(ctx);
		ctx.Defend( e );
		return Task.CompletedTask;
	}

	[InnateTier("1 sun,2 water,1 earth", "In an adjacent land with your presence, Defend E.",1)]
	static public async Task InAdjacent_DefendE(TargetSpaceCtx ctx) {
		int e = GetE(ctx);
		var otherSpace = await ctx.Self.Select(
			$"Defend {e}",
			ctx.Adjacent.Where(ctx.Self.Presence.IsOn),
			Present.Always
		);
		if(otherSpace is not null)
			ctx.Target(otherSpace).Defend( e );
	}

	[InnateTier("1 sun,1 animal", "In target land or adjacent land with your presence, gather up to E Dahan.",2)]
	static public async Task GatherDahan(TargetSpaceCtx ctx) {
		int e = GetE(ctx);
		var otherSpace = await ctx.Self.Select(
			$"Gather up to {e} Dahan",
			ctx.Range(1).Where(ctx.Self.Presence.IsOn),
			Present.Always
		);
		if( otherSpace is not null )
			await ctx.Target(otherSpace).GatherUpToNDahan(e);
	}

	[InnateTier("1 sun,1 water,2 plant", "Remove up to E Health worth of Invaders.",3)]
	static public Task RemoveInvaders(TargetSpaceCtx ctx) {
		int e = GetE(ctx);
		return Cmd.RemoveUpToNHealthOfInvaders(e).ActAsync(ctx);
	}

	// !!! If River play's a Bargain card that removes Energy/Turn, does that effect this?
	static int GetE(TargetSpaceCtx ctx) => ctx.Self.EnergyPerTurn;
}