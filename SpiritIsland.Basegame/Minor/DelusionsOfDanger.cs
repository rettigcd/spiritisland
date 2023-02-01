namespace SpiritIsland.Basegame;

public class DelusionsOfDanger {

	[MinorCard("Delusions of Danger",1,Element.Sun,Element.Moon,Element.Air)]
	[Fast]
	[FromPresence(1,Target.Any)]
	static public Task ActionAsync(TargetSpaceCtx ctx){

		return ctx.SelectActionOption(
			new SpaceAction( "Push 1 Explorer", ctx => ctx.Push( 1, Human.Explorer ) ),
			new SpaceAction( "2 fear", ctx => ctx.AddFear( 2 ) )
		);

	}

}