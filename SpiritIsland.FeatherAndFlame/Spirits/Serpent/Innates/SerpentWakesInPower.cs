namespace SpiritIsland.FeatherAndFlame;

[InnatePower( SerpentWakesInPower.Name ), Slow, Yourself]
public class SerpentWakesInPower {

	public const string Name = "Serpent Wakes in Power";

	[InnateTier( "2 fire,1 water,1 plant","Gain 1 Energy. Other spirits with any Absorbed Presence also gain 1 Energy." )]
	static public Task Option1Async( Spirit self ) {
		// Gain 1 Energy.
		self.Energy += 1;

		// Other spirits with any Absorbed Presence also gain 1 Energy.
		var presence = (SerpentPresence)self.Presence;
		foreach(var spirit in presence.AbsorbedPresences.Distinct())
			spirit.Energy += 1;

		return Task.CompletedTask;
	}

	[InnateTier( "2 water,3 earth,2 plant", "Add 1 presence to range-1.  Other spirits with 2 or more Absobred Presence may do likewise." )]
	static public async Task Option2Async( Spirit self ) {
		await Option1Async( self );

		// Add 1 presence, range-1.
		await Cmd.PlacePresenceWithin(1).ActAsync(self);

		// Other spirits with 2 or more Absorbed Presence may do likewise.
		var presence = (SerpentPresence)self.Presence;
		var qualifyingSpirits = presence.AbsorbedPresences.GroupBy(x=>x).Where(grp=>2<=grp.Count()).Select(grp=>grp.Key);
		foreach(var spirit in presence.AbsorbedPresences.Distinct())
			await Cmd.PlacePresenceWithin( 1 ).ActAsync(spirit);
	}

	[InnateTier("3 fire,3 water,3 earth,3 plant", "Gain a Major Power without Forgetting.  Other Spirits with 3 or more Absorbed Presence may do likewise." )]
	static public async Task Option3Async( Spirit self ) {
		await Option2Async( self );

		// Gain a Major Power without Forgetting.
		await self.Draw.Major(false, 4);

		// Other Spirits with 3 or more Absorbed Presence may do likewise."
		var presence = (SerpentPresence)self.Presence;
		var qualifyingSpirits = presence.AbsorbedPresences.GroupBy(x=>x).Where(grp=>3<=grp.Count()).Select(grp=>grp.Key);
		foreach(var spirit in presence.AbsorbedPresences.Distinct())
			await spirit.Draw.Major( false, 4 );
	}

}
