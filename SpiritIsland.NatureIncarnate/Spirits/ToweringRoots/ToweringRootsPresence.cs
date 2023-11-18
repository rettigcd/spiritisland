namespace SpiritIsland.NatureIncarnate;

public class ToweringRootsPresence : IncarnaPresence {
	public ToweringRootsPresence(Spirit spirit) 
		:base(spirit,
			new PresenceTrack( Track.Energy0, Track.Energy2, Track.EarthEnergy, Track.Energy4, Track.PlantEnergy, Track.Energy6 ),
			new PresenceTrack( Track.Card1, Track.Card2, Track.SunEnergy, Track.Card3, Track.PlantEnergy, Track.Card4 ),
			new ToweringRootsIncarna( spirit )
		) {
		}
}
