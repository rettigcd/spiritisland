namespace SpiritIsland.NatureIncarnate;

public class BurnBrightestBeforeTheEnd : BlightCard {

	public BurnBrightestBeforeTheEnd()
		:base("Burn Brightest Before the End", 
			// The wording on this card is confusing.  It should read:
			// "Immediately: Each Spirit Places (in 1 of your lands) or Destroys 1 Presence from your Presence Tracks.)", 
			"Immediately: Each Spirit Adds 1 Presence to one of their lands or removes 1 Presence from their Presence Tracks. (Presence removed from Tracks goes to the DestroyedPresence supply.)", 
			2
		) 
	{}

	public override IActOn<GameState> Immediately 
		=> PlaceOrDestroyTrackPresence()
			.ForEachSpirit();

	static public SpiritAction PlaceOrDestroyTrackPresence() => new SpiritAction( 
		$"Place or Destroy 1 Presence from your Presence Tracks",
		async self => {
			// From
			IOption from = (IOption)await self.SelectAsync( A.TrackSlot.ToReveal( "Select Presence to Place or Destroy", self ) );
			if(from == null) return;
			// To
			Space to = await self.SelectLandWithPresence("Place presence or Destroy it","Destroy it!");

			if(to == null) {
				// destroy
				await self.Presence.TakeFromAsync( from );
				++self.Presence.Destroyed;
			}
            else
				// place
				await self.Presence.PlaceAsync(from,to);
		}
	);


}
