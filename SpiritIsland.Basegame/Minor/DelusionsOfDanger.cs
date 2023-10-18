namespace SpiritIsland.Basegame;

public class DelusionsOfDanger {

	[MinorCard("Delusions of Danger",1,Element.Sun,Element.Moon,Element.Air),Fast,FromPresence(1,Target.Any)]
	[Instructions( "Push 1 Explorer. -or- 2 Fear" ), Artist( Artists.MoroRogers )]
	static public Task ActionAsync(TargetSpaceCtx ctx){

		return ctx.SelectActionOption(
			new SpaceCmd( "Push 1 Explorer", ctx => ctx.Push( 1, Human.Explorer ) ),
			new SpaceCmd( "2 fear", ctx => ctx.AddFear( 2 ) )
		);

	}

}