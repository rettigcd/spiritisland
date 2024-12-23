namespace SpiritIsland.JaggedEarth;

[InnatePower(Name)]
[Fast, FromIncarna]
public class SocietyDissolvesAtTheBeckoningHeart {

	public const string Name = "Society Dissolves at the Beckoning Heart";

	[InnateTier("1 moon", "For every 3 Explorers/Dahan, Downgrade a different Town/City.", 0)]
	static public async Task Option1(TargetSpaceCtx ctx) {
		// For every 3 Explorers/Dahan, Downgrade a different Town/City.
		int downgradeCount = ctx.Space.SumAny(Human.Explorer,Human.Dahan) / 3;
		while( downgradeCount > 0 && await ReplaceInvader.Downgrade1(ctx.Self,ctx.Space,Present.Done, Human.Town_City) ) {
			--downgradeCount;
		}
	}

	[InnateTier("3 moon,1 air", "Gather 1 Explorer/Town and 1 Dahan.", 1)]
	static public Task Option2(TargetSpaceCtx ctx) {
		// Gather 1 Explorer/Town and 1 Dahan.
		return ctx.Gatherer
			.AddGroup(1,Human.Explorer_Town)
			.AddGroup(1,Human.Dahan)
			.DoN();
	}

	[InnateTier("4 moon,1 air", "Once for every 6 Explorers/Dahan present (at your Incarna): Gather any Invader within Range equal to the amount of Air you have one land towards your Incarna.", 2)]
	static public async Task Option3(TargetSpaceCtx ctx) {
		// Once for every 6 Explorers/Dahan present (at your Incarna):
		int gatherCount = ctx.Space.SumAny(Human.Explorer, Human.Dahan) / 6;

		int airCount = ctx.Self.Elements[Element.Air];
		var dist = ctx.Space.CalcDistances(airCount);
		// Gather any Invader within Range equal to the amount of Air you have one land towards your Incarna.
		Move[] moves = GetMoveOptions(ctx, airCount, dist);
		int usedCount = 0;
		while( usedCount < gatherCount && 0 < moves.Length ) {
			var move = await ctx.Self.Select($"Gather {usedCount+1} of {gatherCount}", moves, Present.Done);
			if( move is null ) break;
			await move.Apply();
			// next
			moves = GetMoveOptions(ctx, airCount, dist);
			++usedCount;
		}

		static Move[] GetMoveOptions(TargetSpaceCtx ctx, int airCount, Dictionary<Space, int> dist) {
			return ctx.Space.Range(airCount)
				.SelectMany(s => s.SpaceTokensOfAnyTag(Human.Invader))
				.BuildMoves(st => st.Space.Adjacent.Where(adj => dist.ContainsKey(adj) && dist[adj] < dist[st.Space]))
				.ToArray();
		}
	}

}