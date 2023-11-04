namespace SpiritIsland.NatureIncarnate;

public class SunPresence : SpiritPresence {

	static public readonly SpecialRule PlacementRule = new SpecialRule( "Home of the Island's Heart", "Your presence may only be added/moved to lands that are inland." );

	public SunPresence()
		:base(
			new PresenceTrack( 
				Track.Energy1, 
				Track.MkEnergy( 2, Element.Sun ),
				Track.MkEnergy( 3, Element.Fire ),
				Track.SunEnergy,
				Track.MkEnergy( 4, Element.Any ),
				Track.Energy5
			),
			new PresenceTrack( 
				Track.Card1,
				Track.Card1,
				Track.Card2, 
				Track.SunEnergy, 
				Track.Card3,
				Track.CardReclaim1,
				Track.Card4
			)
		)
	{ }

	public override void SetSpirit( Spirit spirit ) {
		base.SetSpirit( spirit );
//		Token = new EnthrallTheForeignExplorers( spirit );
	}

}