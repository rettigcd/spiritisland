namespace SpiritIsland.BranchAndClaw;

[InnatePower(Name), Slow, FromPresence(1)]
class EncircleTheUnsuspectingPrey {

	public const string Name = "Encircle the Unsuspecting Prey";

	[InnateTier("1 plant,2 animal", "You may Gather 1 Beasts into target or an adjacent land.If you do, 1 Damage in that land.")]
	static public async Task Option1(TargetSpaceCtx ctx) {
		// You may Gather 1 Beasts into target or an adjacent land. If you do, 1 Damage in that land.
		HashSet<Space> dstOptions = ctx.Space.InOrAdjacentTo.ToHashSet();
		
		var moveOptions = ctx.Space.Range(2).Where(s=>s.Beasts.Any)
			.SelectMany(s=>s.Adjacent.Intersect(dstOptions).Select(to=> new Move{ Source=new SpaceToken(s,Token.Beast), Destination=to }))
			.ToArray();

		var move = await ctx.Self.SelectAsync(new A.Move("Gather Beast", moveOptions, Present.Done));
		if( move is null) return;
		await move.Source.MoveTo(move.Destination);
		await ctx.Target(move.Destination).DamageInvaders(1);
	}

	[InnateTier("1 moon,3 animal", "If at least 2 Beasts are within 1 Range(of target land), 1 Damage.",1)]
	static public async Task Option2(TargetSpaceCtx ctx) {
		// If at least 2 Beasts are within 1 Range(of target land), 1 Damage.
		if( 2 <= ctx.Space.Range(1).Sum(x=>x.Beasts.Count))
			await ctx.DamageInvaders(1);
	}

	[InnateTier("4 moon,1 plant,4 animal", "For each adjacent land with Beasts, 1 Damage.",2)]
	static public Task Option3(TargetSpaceCtx ctx) {
		return ctx.DamageInvaders( ctx.Space.Adjacent.Count(adj=>adj.Beasts.Any) );
	}

}
