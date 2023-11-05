namespace SpiritIsland.NatureIncarnate;

public class AddPresenceOrGainMajor : SpiritAction {

	public AddPresenceOrGainMajor():base( "AddPresenceOrGainMajor" ) { }

	public override Task ActAsync( SelfCtx ctx ) {
		return Cmd.Pick1(
			Cmd.PlacePresenceWithin(2),
			DrawMajorWithoutForgetting
		).ActAsync(ctx);
	}

	static SpiritAction DrawMajorWithoutForgetting => new SpiritAction( "Draw Major without Forgetting",
		ctx => DrawFromDeck.DrawInner( ctx.Self, GameState.Current.MajorCards, 1, 1 )
	);

}