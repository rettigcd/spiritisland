namespace SpiritIsland.JaggedEarth;

public class HomeOfTheIslandsHeart( Spirit spirit ) 
	: SpiritPresence( spirit,
		new PresenceTrack( Track.Energy1, Track.Energy2, Track.MoonEnergy, Track.MkEnergy( 3, Element.Plant ), Track.MkEnergy( 4, Element.Air ), Track.Energy5Reclaim1 ),
		new PresenceTrack( Track.Card1, Track.Card2, Track.AnimalEnergy, Track.Card3, Track.Card4, Track.Card5Reclaim1 ),
		new EnthrallTheForeignExplorers( spirit )
	)
{
	public const string Name = "Home of the Island's Heart";
	const string Description = "Your presence may only be added/moved to lands that are inland.";
	static public SpecialRule PlacementRule => new SpecialRule( Name, Description );

	// !! ?? this is interesting - if all ITokens implemented CanBePlacedOn,
	// could we do away with .IsInPlay?
	// And would that be better than what we currently have
	// AND - shouldn't this be on the Token and not on SpiritPresence?
	public override bool CanBePlacedOn( Space space ) => ActionScope.Current.TerrainMapper.IsInland( space );

}