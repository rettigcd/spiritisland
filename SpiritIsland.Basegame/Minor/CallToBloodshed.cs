namespace SpiritIsland.Basegame;

public class CallToBloodshed {

	public const string Name = "Call to Bloodshed";

	[MinorCard(Name,1,Element.Sun,Element.Fire,Element.Animal),Slow,FromPresence(2,Target.Dahan)]
	[Instructions( "1 Damage per Dahan. -or- Gather up to 3 Dahan." ), Artist( Artists.JorgeRamos )]
	static public Task Act(TargetSpaceCtx ctx){
		return ctx.SelectActionOption(
			new SpaceAction( "1 damage per dahan", ctx => ctx.DamageInvaders( ctx.Dahan.CountAll ) ),
			new SpaceAction( "gather up to 3 dahan", ctx => ctx.GatherUpToNDahan( 3 ) )
		);
	}
}