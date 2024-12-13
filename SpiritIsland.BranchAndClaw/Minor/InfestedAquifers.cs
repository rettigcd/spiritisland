namespace SpiritIsland.BranchAndClaw;

public class InfestedAquifers {

	[MinorCard( "Infested Aquifers", 0, Element.Moon, Element.Water, Element.Earth, Element.Animal ),Slow,FromPresence( 0 )]
	[Instructions( "If target land has any Disease, 1 Damage to each Invader. -or- If target land is Mountain / Wetland, 1 Fear and add 1 Disease." ), Artist( Artists.NolanNasser )]
	static public Task ActAsync( TargetSpaceCtx ctx ) {

		return ctx.SelectActionOption(
			new SpaceAction( "1 damage to each invader"
				, ctx => ctx.DamageEachInvader( 1 )
			).OnlyExecuteIf( x=>x.Disease.Any ),
			new SpaceAction( "1 fear and 1 disease", async ctx => { await ctx.AddFear(1); await ctx.Disease.AddAsync(1); } )
				.OnlyExecuteIf( x => x.IsOneOf(Terrain.Mountain,Terrain.Wetland) )
		);
	}

}