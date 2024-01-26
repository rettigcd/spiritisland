namespace SpiritIsland;

using XFilter = TargetSpaceCtxFilter;

public static class Is {
	static public XFilter AnyLand => new XFilter( "any land", _ => true );
	static public XFilter Inland => new XFilter( "Inland", x => x.IsInland );
	static public XFilter Coastal => new XFilter( "coastal land", ctx => ctx.IsCoastal );
	static public XFilter AdjacentToBlight => new XFilter( "land adjacent to blight", spaceCtx => spaceCtx.AdjacentCtxs.Any( adjCtx => adjCtx.Tokens.Blight.Any ) );
	static public XFilter NotRavageCardMatch => new XFilter( "land that does not match Ravage card", ctx => !MatchingSlotCard( ctx, Deck.Ravage ) );
	static public XFilter NotBuildCardMatch => new XFilter( "land that does not match Build card", x => !MatchingSlotCard( x, Deck.Build ) );
	static public XFilter RavageCardMatch => new XFilter( "matching a Ravage card", ctx => MatchingSlotCard( ctx, Deck.Ravage ) );
	static public XFilter ExploreCardMatch => new XFilter( "matching an Explore card", ctx => MatchingSlotCard( ctx, Deck.Explore ) );

	static public XFilter NotExploreOrBuildCardMatch => new XFilter( 
		"land that does not match Explore or Build card", 
		x => {
			var deck = GameState.Current.InvaderDeck;
			return !MatchingSlotCard( x, deck.Build ) && !MatchingSlotCard( x, deck.Explore );
		}
	);

	static bool MatchingSlotCard( TargetSpaceCtx ctx, InvaderSlot slot )
		=> slot.Cards.Any( card => card.Flipped && card.MatchesCard( ctx.Tokens ) );

	// Spirits 
	static public SpiritFilter AnySpirit => new SpiritFilter( "Spirit", _ => true );

	static InvaderDeck Deck => GameState.Current.InvaderDeck;
}