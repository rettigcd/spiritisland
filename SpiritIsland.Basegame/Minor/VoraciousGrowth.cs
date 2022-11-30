namespace SpiritIsland.Basegame;

public class VoraciousGrowth {

	[MinorCard("Voracious Growth",1,Element.Water,Element.Plant)]
	[Slow]
	[FromSacredSite(1,Target.Jungle, Target.Wetland )]
	static public Task ActAsync(TargetSpaceCtx ctx){

		return ctx.SelectActionOption(
			new SpaceAction( "2 Damage", ctx => ctx.DamageInvaders( 2 ) ).Matches( x => x.HasInvaders ),
			new SpaceAction( "Remove 1 Blight", ctx => ctx.RemoveBlight() ).Matches( x => x.HasBlight )
		);

	}

}