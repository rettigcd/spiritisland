namespace SpiritIsland.NatureIncarnate;

/// <remarks>Growth Only</remarks>
public class PlacePresenceOrGainMajor : SpiritAction {

	public PlacePresenceOrGainMajor():base( "Add Presence or Gain Major" ) { }

	public override Task ActAsync( Spirit self ) {
		return Cmd.Pick1(
			new PlacePresence(2),
			DrawMajorWithoutForgetting
		).ActAsync(self);
	}

	static SpiritAction DrawMajorWithoutForgetting => new SpiritAction( "Draw Major without Forgetting",
		self => DrawFromDeck.DrawInner( self, GameState.Current.MajorCards!, 1, 1 )
	);

}