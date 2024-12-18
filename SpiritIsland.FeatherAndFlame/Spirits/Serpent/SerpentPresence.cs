namespace SpiritIsland.FeatherAndFlame;

public class SerpentPresence : SpiritPresence {


	readonly static Track fakeEarth = new Track("earth energy" ) {
		Icon = new IconDescriptor { Text = "X" }
	};

	public SerpentPresence(Spirit spirit):base(spirit,
		new PresenceTrack( Track.Energy1, Track.FireEnergy, Track.AnyEnergy, Track.Reclaim1Energy, Track.EarthEnergy, Track.Energy6, Track.AnyEnergy, Track.MkEnergy(12) ),
		new PresenceTrack( Track.Card1,   Track.MkCard(Element.Moon), Track.Card2, Track.MkCard( Element.Water ), fakeEarth, Track.Card4,   Track.Card5Reclaim1 )
	){
		Energy.TrackRevealedAsync += Energy_TrackRevealed;
	}

	async Task Energy_TrackRevealed( TrackRevealedArgs obj ) {
		if( obj.Track == Track.EarthEnergy )
			await CardPlays.RevealAsync(fakeEarth);
	}

	public override IEnumerable<TokenLocation> RevealOptions() {
		if(MaxPresenceOnBoard == TotalOnIsland() ) yield break;

		Track? energyNext = Energy.RevealOptions.FirstOrDefault();
		if( energyNext != null && (energyNext != Track.EarthEnergy || CardPlays.Revealed.Count() == 4 ) )
			yield return new TrackPresence(energyNext,Token);

		Track? cardNext = CardPlays.RevealOptions.FirstOrDefault();
		if( cardNext != null && cardNext != fakeEarth )  // don't let them select the fake earth
			yield return new TrackPresence(cardNext,Token);

	}

	public List<Spirit> AbsorbedPresences = []; // don't let it grow past 6

	public int MaxPresenceOnBoard => new int[]{5,7,8,10,11,12,13 }[AbsorbedPresences.Count];

	protected override object? CustomMementoValue {
		get => AbsorbedPresences.ToArray();
		set { AbsorbedPresences.SetItems( (Spirit[]?)value! ); }
	}


}