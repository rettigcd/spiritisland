namespace SpiritIsland.BranchAndClaw;

public class SwarmingWasps {

	[MinorCard( "Swarming Wasps", 0, Element.Fire, Element.Air, Element.Animal ),Fast,FromPresence( 1, Target.NoBlight )]
	[Instructions( "Add 1 Beasts. -or- If target land has Beasts, Push up to 2 Explorer." ), Artist( Artists.JoshuaWright )]
	static public Task ActAsync( TargetSpaceCtx ctx ) {

		return ctx.SelectActionOption(
			new SpaceCmd( "Add 1 beast", ctx => ctx.Beasts.AddAsync(1) ),
			new SpaceCmd( "Push up to 2 explorers", ctx => ctx.PushUpTo( 2, Human.Explorer ) )
				.OnlyExecuteIf( x=>x.Beasts.Any )
		);

	}

}