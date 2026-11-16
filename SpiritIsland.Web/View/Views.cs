namespace SpiritIsland.Web.View;

// Plain records only - no web types - so the projection can move into a library if the engine ever runs in the browser.

/// <summary> What a request returns: where the game is and what to answer next. </summary>
/// <param name="CheckpointVersion">Send this back with the next request; a request made against an older one is stale.</param>
/// <param name="Picks">The picks made since that checkpoint (the action in progress). The client replaces its own list with this one.</param>
/// <param name="Decision">Null when the game is over.</param>
/// <param name="Result">Set when the game is over.</param>
public record OracleResponse( string GameId, int CheckpointVersion, string[] Picks, GameView View, DecisionView? Decision, ResultView? Result );

/// <summary> Phase 1 keeps this minimal; the full player view is Phase 2 of docs/WebClient-Plan.md. </summary>
public record GameView( int Round, string Phase );

public record DecisionView( string Prompt, OptionView[] Options );

/// <param name="Id">The option's text, made unique within its decision. This is what a pick sends.</param>
public record OptionView( string Id, string Label );

public record ResultView( string Outcome, string Cause );

/// <summary> Setup choices for a new game. </summary>
public record SetupOptions( string[] Spirits, string[] Boards, Dictionary<string, int[]> Adversaries );

/// <summary> The oracle call: the checkpoint the client built on and the picks it has made since. </summary>
public record DecideRequest( int CheckpointVersion, string[] Picks );

public record ErrorView( string Error );
