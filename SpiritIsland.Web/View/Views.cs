namespace SpiritIsland.Web.View;

// Plain records only - no web types - so the projection can move into a library if the engine ever runs in the browser.

/// <summary> What a request returns: where the game is and what to answer next. </summary>
/// <param name="CheckpointVersion">Send this back with the next request; a request made against an older one is stale.</param>
/// <param name="Picks">The picks made since that checkpoint (the action in progress). The client replaces its own list with this one.</param>
/// <param name="StaticVersion">The client fetches the static view again when this differs from the one it holds.</param>
/// <param name="Decision">Null when the game is over.</param>
/// <param name="Result">Set when the game is over.</param>
public record OracleResponse( string GameId, int CheckpointVersion, string[] Picks, string StaticVersion, GameView View, DecisionView? Decision, ResultView? Result );

/// <summary>
/// Everything a player may see of the game right now. Built by <see cref="GameViewBuilder"/>, which never reads what is
/// hidden (invader deck order, face-down fear cards, the unflipped blight card): only counts of those are here.
/// </summary>
/// <param name="ViewerSpiritId">Which of <paramref name="Spirits"/> belongs to the person looking.</param>
public record GameView(
	int Round, string Phase,
	FearView Fear, BlightView Blight, InvadersView Invaders,
	string ViewerSpiritId, SpiritView[] Spirits,
	SpaceView[] Spaces );

public record FearView( int TerrorLevel, int FearPool, int FearPoolMax, int FacedownCardsRemaining, int EarnedCardsPending );

/// <param name="Card">Null until the card flips: it is set up healthy side up without looking at the back.</param>
public record BlightView( bool CardFlipped, string? Card, string? BlightedIslandEffect, int BlightRemainingOnCard, int BlightCardsRemaining );

/// <summary> Invader cards by their text, in the order of their slot. The deck's order and contents stay hidden. </summary>
public record InvadersView( string[] Explore, string[] Build, string[] Ravage, string[] Discards, int UnrevealedCount );

/// <param name="Id">The spirit's name.</param>
/// <param name="Elements">A one-line summary of the elements this turn.</param>
/// <param name="GrowthGroupsUsed">The growth groups already used this turn, as their text.</param>
public record SpiritView(
	string Id, string Name, int Energy, string Elements,
	PresenceView Presence, string[] GrowthGroupsUsed,
	CardView[] Hand, CardView[] InPlay, CardView[] Discard,
	InnateView[] Innates );

/// <param name="Energy">Which slots of the energy track are revealed.</param>
/// <param name="CardPlays">Which slots of the card-play track are revealed.</param>
/// <param name="SacredSites">The lands with 2 or more presence, by space id.</param>
public record PresenceView( TrackView Energy, TrackView CardPlays, int EnergyPerTurn, int CardPlaysPerTurn, int Destroyed, string[] SacredSites );

/// <param name="Revealed">Indexes into the track's slots (listed in the static view) of the slots that are revealed.</param>
public record TrackView( int[] Revealed );

/// <param name="Id">The card's title, unique within a spirit.</param>
public record CardView( string Id, string Name, int Cost, string Speed, string Elements );

/// <param name="TierStatus">For each tier, whether this turn's elements meet it.</param>
public record InnateView( string Name, string[] TierStatus );

/// <param name="Id">The space's label.</param>
public record SpaceView( string Id, TokenView[] Tokens );

/// <param name="Id">The token's text, unique among the tokens on its space.</param>
/// <param name="Image">Which picture to draw: the token's image, or "Presence:" and the spirit's name for presence.</param>
/// <param name="Badge">1 or 2 letters shown on the token (damage, variety).</param>
/// <param name="X">Where to draw it, in the same board coordinates as the space's corners.</param>
public record TokenView( string Id, string Image, string Label, int Count, string Badge, int Strife, bool SacredSite, double X, double Y );

public record DecisionView( string Prompt, OptionView[] Options );

/// <param name="Id">The option's text, made unique within its decision. This is what a pick sends.</param>
/// <param name="Target">What on the board or in a panel this option is, so the client can highlight it.</param>
public record OptionView( string Id, string Label, OptionTarget Target );

/// <summary>
/// What an option points at. One record with a <paramref name="Kind"/> and the fields that kind uses, not a class
/// hierarchy, so it maps cleanly to OpenAPI and TypeScript. Kinds and their fields:
/// "space" (Space), "spaceToken" (Space, Token), "trackPresence" (Spirit, Track, Slot: presence sitting on a track slot),
/// "move" (a token going somewhere: the source's fields - Space and Token, or Spirit, Track and Slot for presence leaving
/// a track, or just Spirit for presence coming back from the destroyed pile - plus ToSpace, the destination, which is
/// empty when it isn't a space),
/// "track" (Spirit, Track, Slot: a slot to reveal or return), "destroyedPresence" (Spirit), "card" (Spirit, Card),
/// "innate" (Spirit, Card), "action" (an action to resolve, like a growth option: nothing to point at, use the label),
/// "other" (Done, a choice with no picture).
/// </summary>
/// <param name="Space">A space id (its label). For a move, the space the token is leaving.</param>
/// <param name="Token">A token id: the token's text, as in <see cref="TokenView.Id"/>.</param>
/// <param name="Spirit">A spirit id, for what belongs to a spirit.</param>
/// <param name="Card">A card's or innate power's title.</param>
/// <param name="Track">"energy" or "cardPlays".</param>
/// <param name="Slot">Index into that track's slots (listed in the static view).</param>
/// <param name="ToSpace">For a move, the space id of the destination.</param>
public record OptionTarget( string Kind, string? Space = null, string? Token = null, string? Spirit = null, string? Card = null, string? Track = null, int? Slot = null, string? ToSpace = null );

public record ResultView( string Outcome, string Cause );

/// <summary> Setup choices for a new game. </summary>
public record SetupOptions( string[] Spirits, string[] Boards, Dictionary<string, int[]> Adversaries );

/// <summary> The oracle call: the checkpoint the client built on and the picks it has made since. </summary>
public record DecideRequest( int CheckpointVersion, string[] Picks );

public record ErrorView( string Error );

/// <summary>
/// What rarely changes during a game: the island's geometry, the setup, and each spirit's tracks, growth and innate powers.
/// Fetched once, and again only when <see cref="StaticVersion"/> (also carried by every response) changes.
/// </summary>
/// <param name="StaticVersion">A hash of everything else here; it changes when, for instance, lands are woven together.</param>
public record StaticView( string StaticVersion, SetupView Setup, BoundsView WorldBounds, SpaceGeometry[] Spaces, SpiritStatic[] Spirits );

public record SetupView( string[] Boards, AdversaryView? Adversary );

/// <param name="Rules"> The adversary's rules at this level, as text. </param>
public record AdversaryView( string Name, int Level, string Rules );

/// <summary> A rectangle in board coordinates. </summary>
public record BoundsView( double X, double Y, double Width, double Height );

public record PointView( double X, double Y );

/// <param name="Id">The space's label.</param>
/// <param name="Terrain">Its terrains joined with "/", like "Jungle/Wetland".</param>
/// <param name="Corners">The outline, in order, in board coordinates.</param>
/// <param name="NameLocation">Where to write the space's name.</param>
/// <param name="TokenSize">How big to draw a token, in board coordinates.</param>
/// <param name="Adjacent">The ids of the spaces next to it.</param>
public record SpaceGeometry( string Id, string Terrain, PointView[] Corners, PointView NameLocation, double TokenSize, string[] Adjacent );

/// <param name="Id">The spirit's name.</param>
public record SpiritStatic(
	string Id, string Name,
	SlotView[] EnergyTrack, SlotView[] CardPlayTrack,
	GrowthGroupView[] GrowthGroups, InnateStatic[] Innates, RuleView[] SpecialRules );

/// <summary> One slot of a presence track. A track's revealed slots are indexes into this list. </summary>
/// <param name="Id">The slot's code, like "2 energy" or "1 cardplay".</param>
/// <param name="Energy">Energy gained each turn from this slot, if it gives any.</param>
/// <param name="CardPlays">Extra card plays each turn from this slot, if it gives any.</param>
/// <param name="Elements">Elements gained each turn from this slot.</param>
public record SlotView( string Id, string Label, int? Energy, int? CardPlays, string[] Elements );

/// <param name="Id">The group as text, which is what <see cref="SpiritView.GrowthGroupsUsed"/> lists.</param>
/// <param name="Actions">The actions in the group, as text.</param>
public record GrowthGroupView( string Id, string[] Actions, int EnergyChange );

public record InnateStatic( string Name, string Speed, string Target, string Range, string Instructions, TierView[] Tiers );

/// <param name="Threshold">The elements needed, as text.</param>
public record TierView( string Threshold, string Effect );

public record RuleView( string Title, string Description );
