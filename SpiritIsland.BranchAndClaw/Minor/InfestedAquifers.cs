using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class InfestedAquifers {

		[MinorCard( "Infested Aquifers", 0, Speed.Slow, Element.Moon, Element.Water, Element.Earth, Element.Animal )]
		[FromPresence( 0 )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {

			return ctx.SelectActionOption(
				new ActionOption( "1 damage to each invader"
					, () => ctx.PowerInvaders.ApplyDamageToEach( 1, Invader.City, Invader.Town, Invader.Explorer )
					, ctx.Tokens.Disease().Any
				),
				new ActionOption( "1 fear and 1 disease"
					, () => { ctx.Tokens.Disease().Count++; return Task.CompletedTask; }
					, ctx.IsOneOf(Terrain.Mountain,Terrain.Wetland)
				)
			);
		}

	}

}
