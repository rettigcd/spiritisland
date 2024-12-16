namespace SpiritIsland.Basegame;

public class SteadyRegeneration( Spirit spirit ) 
	: SpiritPresence( spirit,
		new PresenceTrack( Track.Energy0, Track.Energy1, Track.PlantEnergy, Track.Energy2, Track.Energy2, Track.PlantEnergy, Track.Energy3 ),
		new PresenceTrack( Track.Card1, Track.Card1, Track.Card2, Track.Card2, Track.Card3, Track.Card4 ),
		new ChokeTheLandWithGreen((ASpreadOfRampantGreen)spirit)
	)
{
	public const string Name = "Steady Regeneration";
	const string Description = "When adding Presence to the board via Growth, you may optionally use your destroyed Presence. If the island is Healthy, do so freely. If the island is Blighted, doing so costs 1 Energy per destroyed Presence you add.";
	static public SpecialRule Rule => new SpecialRule(Name,Description);


	public override IEnumerable<TokenLocation> RevealOptions() {
		return CanAddFromDestroyed() ? base.RevealOptions().Append( Destroyed )
			: base.RevealOptions();
	}

	// !!! BUG (I think) - do we --Energy when adding blight when island is blighted?

	bool CanAddFromDestroyed() => 0 < Destroyed.Count && HasRequiredEnergy;
	bool HasRequiredEnergy => 0 < Self.Energy || IslandIsHealthy;
	static bool IslandIsHealthy => !GameState.Current.BlightCard.CardFlipped;
}