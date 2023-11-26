namespace SpiritIsland.Basegame;

public class RampantGreenPresence : SpiritPresence {

	public RampantGreenPresence(Spirit spirit) 
		: base( spirit,
			new PresenceTrack( Track.Energy0, Track.Energy1, Track.PlantEnergy, Track.Energy2, Track.Energy2, Track.PlantEnergy, Track.Energy3 ),
			new PresenceTrack( Track.Card1, Track.Card1, Track.Card2, Track.Card2, Track.Card3, Track.Card4 ),
			new ChokeTheLandWithGreen((ASpreadOfRampantGreen)spirit)
		) {
	}

	public override IEnumerable<Track> RevealOptions() {
		return CanAddFromDestroyed() ? base.RevealOptions().Append(Track.Destroyed)
			: base.RevealOptions();
	}

	// !!! BUG (I think) - do we --Energy when adding blight when island is blighted?

	bool CanAddFromDestroyed() => 0 < Destroyed	&& HasRequiredEnergy;
	bool HasRequiredEnergy => 0 < Self.Energy || IslandIsHealthy;
	static bool IslandIsHealthy => !GameState.Current.BlightCard.CardFlipped;
}