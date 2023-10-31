namespace SpiritIsland.NatureIncarnate;

public class AddPresenceOrGainMajor : GrowthActionFactory {
	public override Task ActivateAsync( SelfCtx ctx ) {
		return Cmd.Pick1(
			Cmd.PlacePresenceWithin(new TargetCriteria(2),false),
			DrawMajorWithoutForgetting
		).Execute(ctx);
	}

	static SelfCmd DrawMajorWithoutForgetting => new SelfCmd( "Draw Major without Forgetting",
		ctx => DrawFromDeck.DrawInner( ctx.Self, GameState.Current.MajorCards, 1, 1 )
	);

}