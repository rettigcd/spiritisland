namespace SpiritIsland.Basegame;

[InnatePower(Name), Fast, AnotherSpirit]
public class BoonOfSunshinesPromise {

	public const string Name = "Boon of Sunshine's Promise";

	static public void InitAspect(Spirit spirit) {
		// Add new Innate
		spirit.InnatePowers = [.. spirit.InnatePowers, InnatePower.For(typeof(BoonOfSunshinesPromise))];
	}

	[InnateTier("2 sun", "Target Spirit gains Energy equal to 1 less than the highest uncovered number on your Energy track.")]
	static public Task ShareEnergy(TargetSpiritCtx ctx) {
		// !!! If River play's a Bargain card that removes Energy/Turn, does that effect this?
		ctx.Other.Energy += Math.Max(0,ctx.Self.EnergyPerTurn-1);
		return Task.CompletedTask;
	}

	[InnateTier("3 sun,1 water", "You also gain that much Energy.", 1)]
	static public Task YouGetEnergyToo(TargetSpiritCtx ctx) {
		ctx.Self.Energy += Math.Max(0, ctx.Self.EnergyPerTurn - 1);
		return Task.CompletedTask;
	}

	[InnateTier("4 sun,2 water", "Target Spirit may Remove 1 Blight from one of their lands with Dahan.", 2)]
	static public async Task RemoveBlightFromDahanLand(TargetSpiritCtx ctx) {
		var blightTokens = ctx.Other.Presence.Lands
			.Where(s => s.Blight.Any && s.Dahan.Any)
			.Select(s=>new SpaceToken(s,Token.Blight));
		var blightToken = await ctx.Other.Select(new A.SpaceTokenDecision("Select Blight to Remove", blightTokens, Present.Done));
		if(blightToken is not null ) {
			await blightToken.Remove(); // non-generic
			// await blightToken.RemoveAsync(); // generic method
		}
	}

}