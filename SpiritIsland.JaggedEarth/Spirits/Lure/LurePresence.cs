namespace SpiritIsland.JaggedEarth;

public class LurePresence : SpiritPresence {

	static public readonly SpecialRule PlacementRule = new SpecialRule( "Home of the Island's Heart", "Your presence may only be added/moved to lands that are inland." );

	public LurePresence()
		:base(
			new PresenceTrack( Track.Energy1, Track.Energy2, Track.MoonEnergy, Track.MkEnergy( 3, Element.Plant ), Track.MkEnergy( 4, Element.Air ), Track.Energy5Reclaim1 ),
			new PresenceTrack( Track.Card1, Track.Card2, Track.AnimalEnergy, Track.Card3, Track.Card4, Track.Card5Reclaim1 )
		)
	{ }

	public override void SetSpirit( Spirit spirit ) {
		base.SetSpirit( spirit );
		Token = new EnthrallTheForeignExplorers( spirit );
	}

	public override bool CanBePlacedOn( SpaceState space, TerrainMapper tm ) => tm.IsInland( space );

}