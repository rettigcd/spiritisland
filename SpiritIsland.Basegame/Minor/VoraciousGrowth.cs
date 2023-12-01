namespace SpiritIsland.Basegame;

public class VoraciousGrowth {

	[MinorCard("Voracious Growth",1,Element.Water,Element.Plant),Slow,FromSacredSite(1,Filter.Jungle, Filter.Wetland )]
	[Instructions( "2 Damage. -or- Remove 1 Blight." ),Artist(Artists.CariCorene )]   
		static public Task ActAsync(TargetSpaceCtx ctx){

		return ctx.SelectActionOption(
			new SpaceCmd( "2 Damage", ctx => ctx.DamageInvaders( 2 ) ).OnlyExecuteIf( x => x.HasInvaders ),
			new SpaceCmd( "Remove 1 Blight", ctx => ctx.RemoveBlight() ).OnlyExecuteIf( x => x.HasBlight )
		);

	}

}