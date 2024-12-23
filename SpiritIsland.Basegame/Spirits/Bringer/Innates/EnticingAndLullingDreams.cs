namespace SpiritIsland.Basegame;

[InnatePower(Name), Fast]
[FromPresence(1)]
public class EnticingAndLullingDreams {

	public const string Name = "Enticing and Lulling Dreams";

	[InnateTier("2 moon,1 air", "Each Invader/Dahan does -1 Damage while in target land.",0)]
	static public Task OneLessDamage(TargetSpaceCtx ctx) {
		// Each Invader/Dahan does -1 Damage while in target land.
		ReduceAttack(ctx, 1); // Grp:0
		return Task.CompletedTask;
	}

	[InnateTier("2 moon,3 animal", "Gather up to 1 Explorer/Town.",1)]
	static public Task TwoFear(TargetSpaceCtx ctx) {
		// Gather up to 1 Explorer/Town.
		return ctx.GatherUpTo(1,Human.Explorer_Town); // Grp:1
	}

	[InnateTier("3 moon,2 air,1 animal", "Each Invader/Dahan does -1 Damage while in target land.",0)]
	static public Task ThreeFear(TargetSpaceCtx ctx) {
		// Each Invader/ Dahan does - 1 Damage while in target land.
		ReduceAttack(ctx,1+1); // Grp:0
		return Task.CompletedTask;
	}

	static void ReduceAttack(TargetSpaceCtx ctx,int reduction) {
		ctx.Space.Init(new ReduceAttack(1, Human.Dahan, Human.Explorer, Human.Town, Human.City), reduction);
	}

	[InnateTier("4 moon,3 air,2 animal", "Gather up to 4 Explorers/Towns.",1)]
	static public Task Option4(TargetSpaceCtx ctx) {
		// Gather up to 4 Explorers/Towns.
		return ctx.GatherUpTo(1+4,Human.Explorer_Town); // Grp:1
	}



}
