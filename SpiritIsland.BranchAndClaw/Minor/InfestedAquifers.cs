namespace SpiritIsland.BranchAndClaw;

public class InfestedAquifers {

	[MinorCard( "Infested Aquifers", 0, Element.Moon, Element.Water, Element.Earth, Element.Animal )]
	[Slow]
	[FromPresence( 0 )]
	static public Task ActAsync( TargetSpaceCtx ctx ) {

		return ctx.SelectActionOption(
			new SpaceAction( "1 damage to each invader"
				, ctx => ctx.DamageEachInvader( 1 )
			).Matches( x=>x.Disease.Any ),
			new SpaceAction( "1 fear and 1 disease", ctx => { ctx.AddFear(1); ctx.Disease.Add(1); return Task.CompletedTask; } )
				.Matches( x => x.IsOneOf(Terrain.Mountain,Terrain.Wetland) )
		);
	}

}