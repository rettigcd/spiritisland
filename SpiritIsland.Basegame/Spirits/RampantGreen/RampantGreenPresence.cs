namespace SpiritIsland.Basegame;

public class RampantGreenPresence : SpiritPresence {

	public RampantGreenPresence() 
		: base(
			new PresenceTrack( Track.Energy0, Track.Energy1, Track.PlantEnergy, Track.Energy2, Track.Energy2, Track.PlantEnergy, Track.Energy3 ),
			new PresenceTrack( Track.Card1, Track.Card1, Track.Card2, Track.Card2, Track.Card3, Track.Card4 )
		) {
	}

	public override void SetSpirit( Spirit spirit ) {
		base.SetSpirit( spirit );
		Token = new ChokeTheLandWithGreen((ASpreadOfRampantGreen)spirit);
	}

	public IEntityClass Class => throw new NotImplementedException();

	public string Text => throw new NotImplementedException();

	public override IEnumerable<Track> RevealOptions() { 
		var options = base.RevealOptions().ToList();
		if( Destroyed>0 ) options.Add(Track.Destroyed);
		return options;
	}

}