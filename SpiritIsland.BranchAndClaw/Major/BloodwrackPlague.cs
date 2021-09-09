using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class BloodwrackPlague {

		[MajorCard("Bloodwrack Plague",4,Speed.Fast,Element.Water,Element.Earth,Element.Animal)]
		[FromSacredSite(1)]
		static public async Task ActAsync(TargetSpaceCtx ctx ) {
			// add 2 disease
			var disease = ctx.Tokens.Disease();
			disease.Count+=2;

			// for each disease in target land, defend 1 in target and all adjacent lands
			ctx.Defend(1);
			foreach(var adjacent in ctx.PowerAdjacents())
				ctx.GameState.Defend(adjacent,1);

			// if you have 2 earthn 4 animal:
			if(ctx.Self.Elements.Contains("2 earth,4 animal")) { 
				// 2 fear.
				ctx.AddFear(2);
				// For each disease in target land, do 1 damage in target or adjacent land
				int damage = disease.Count;
				var space = await ctx.Self.Action.Decide(new TypedDecision<Space>($"Select space to apply {damage} damage", ctx.Target.Range(1), Present.Always ));
				await ctx.InvadersOn(space).ApplySmartDamageToGroup(damage);
			}
		}

	}

}
