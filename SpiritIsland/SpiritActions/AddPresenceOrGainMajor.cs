namespace SpiritIsland.NatureIncarnate;

public class AddPresenceOrGainMajor : SpiritAction {

	public AddPresenceOrGainMajor():base( "AddPresenceOrGainMajor" ) { }

	public override Task ActAsync( Spirit self ) {
		return Cmd.Pick1(
			Cmd.PlacePresenceWithin(2),
			DrawMajorWithoutForgetting
		).ActAsync(self);
	}

	static SpiritAction DrawMajorWithoutForgetting => new SpiritAction( "Draw Major without Forgetting",
		self => DrawFromDeck.DrawInner( self, GameState.Current.MajorCards, 1, 1 )
	);

}