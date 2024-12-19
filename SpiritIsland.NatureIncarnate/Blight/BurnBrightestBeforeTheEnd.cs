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
			TokenLocation? from = await self.Select( Prompts.SelectPresenceTo("to Place or Destroy"), self.Presence.RevealOptions(), Present.Always );
			if( from is null) return; // Spirit might not have any more presence on track.

			// To
			Space? to = await self.Select("Place presence or Destroy it", self.Presence.Lands, "Destroy it!");

			if(to is null) {
				// destroy
				await from.RemoveAsync();
				++self.Presence.Destroyed.Count;
			}
			else
				// place
				await from.MoveToAsync(to);
		}
	);


}
