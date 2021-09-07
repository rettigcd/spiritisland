using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class InfestedAquifers {

		[MinorCard( "Infested Aquifers", 0, Speed.Slow, Element.Moon, Element.Water, Element.Earth, Element.Animal )]
		[FromPresence( 0 )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {

			return ctx.SelectActionOption(
				new ActionOption( "1 damage to each invader"
					, () => ctx.InvadersOn( ctx.Target ).ApplyDamageToEach( 1, Invader.City, Invader.Town, Invader.Explorer )
					, ctx.Tokens.Has( BacTokens.Disease )
				),
				new ActionOption( "1 fear and 1 disease"
					, () => { ctx.Tokens[BacTokens.Disease]++; return Task.CompletedTask; }
					, ctx.IsOneOf(Terrain.Mountain,Terrain.Wetland)
				)
			);
		}

	}

}
