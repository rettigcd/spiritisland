using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using SpiritIsland.Agent;

namespace SpiritIsland.Mcp;

/// <summary>
/// MCP adapter over <see cref="AgentGame"/>. Games live on disk (see <see cref="GameStore"/>): any number can be in progress,
/// each call names its game by id, and nothing is held in memory between calls.
/// </summary>
[McpServerToolType]
public sealed class GameTools {

	static readonly JsonSerializerOptions Pretty = new() { WriteIndented = false };

	const string GameIdDescription = "Optional: the gameId from create_game (or list_games). Leave out for the game played most recently.";

	[McpServerTool( Name = "get_game_configuration_options", ReadOnly = true )]
	[Description( "Lists the spirits, boards and adversaries (with their valid levels; 0 = escalation only) that create_game accepts." )]
	public static string GetGameConfigurationOptions() => new JsonObject {
		["spirits"] = new JsonArray( [.. GameSetup.Spirits.Select( s => (JsonNode)s )] ),
		["boards"] = new JsonArray( [.. GameSetup.Boards.Select( s => (JsonNode)s )] ),
		["adversaries"] = new JsonObject( GameSetup.Adversaries.Select( kv =>
			KeyValuePair.Create<string, JsonNode?>( kv.Key, new JsonArray( [.. kv.Value.Select( l => (JsonNode)l )] ) ) ) ),
	}.ToJsonString( Pretty );

	[McpServerTool( Name = "create_game" )]
	[Description( "Starts a new single-spirit game and returns its gameId plus the first decision. Games are saved on disk and independent, so several can be played at once: pass the gameId to the other tools (leave it out to mean the game played most recently). Same spirit+board+adversary+seed gives the same setup." )]
	public static string CreateGame(
		[Description( "Spirit name, as listed by get_game_configuration_options" )] string spirit,
		[Description( "Board letter, e.g. A" )] string board,
		[Description( "Integer random seed" )] int seed,
		[Description( "Optional adversary name" )] string? adversary = null,
		[Description( "Adversary level (default 0)" )] int adversaryLevel = 0 )
		=> GameStore.Create( new NewGameRequest( spirit, board, seed, adversary, adversaryLevel ), ( g, id ) => {
			JsonObject json = DecisionNode( g );
			json["gameId"] = id;
			return json.ToJsonString( Pretty );
		} );

	[McpServerTool( Name = "list_games", ReadOnly = true )]
	[Description( "Lists the saved games, most recently played first: gameId, spirit, board, seed, adversary, decisions made and when last played." )]
	public static string ListGames() => GameStore.List().ToJsonString( Pretty );

	[McpServerTool( Name = "get_static_info", ReadOnly = true )]
	[Description( "Returns what rarely changes during a game: setup and adversary rules, a legend for token and card codes, victory conditions by Terror Level, each land's terrain and adjacency, your spirit's special rules, growth groups, presence track slots and innate powers (with tiers), and the full text of your starting cards. Call once per game, and again whenever get_observation's staticVersion differs from the one you have (or you no longer have it in context)." )]
	public static string GetStaticInfo(
		[Description( GameIdDescription )] string? gameId = null )
		=> GameStore.Use( gameId, g => {
			g.GetDecision(); // let setup finish
			return PlayerObservation.BuildStatic( g ).ToJsonString( Pretty );
		} );

	[McpServerTool( Name = "get_observation", ReadOnly = true )]
	[Description( "Returns the current state a human player could see: fear, blight, invader cards (revealed only), your spirit's energy, elements, track progress, hand/in play/discard (names, costs, speeds, elements), innate tier status, and the tokens on each land that has any. What rarely changes is in get_static_info; its staticVersion is included here so you can tell when to fetch it again. Hidden deck contents are never included. "
		+ "Every response has an observationId. Pass the last one you have as sinceId to get only what changed since then, under 'changes': keys left out are unchanged, null means removed, objects list only their changed keys, and arrays, values and each land under 'spaces' come whole. "
		+ "If that id is no longer remembered (only the last 3 are), the full observation comes back with 'full': true. Leave sinceId out whenever you don't have the earlier observation in context." )]
	public static string GetObservation(
		[Description( "Optional: the observationId of the last observation you still have, to receive only the changes since it" )] int? sinceId = null,
		[Description( GameIdDescription )] string? gameId = null )
		=> GameStore.Use( gameId, g => g.Observations.Next( PlayerObservation.Build( g ), sinceId ).ToJsonString( Pretty ) );

	[McpServerTool( Name = "get_decision", ReadOnly = true )]
	[Description( "Returns the pending decision: a prompt, a version, the list of legal options, and the alerts (game log entries other than your own decisions, e.g. fear generated, fear card revealed with its effect, ravages, blight) recorded since the previous decision. An option's text is its ID. A decision with only one option is chosen for you and reported as an alert of kind 'auto'." )]
	public static string GetDecision(
		[Description( GameIdDescription )] string? gameId = null )
		=> GameStore.Use( gameId, g => DecisionNode( g ).ToJsonString( Pretty ) );

	[McpServerTool( Name = "get_card_details", ReadOnly = true )]
	[Description( "Returns the details (cost, speed, elements, range, target, instructions) of a power card you have access to: in your hand, in play, in your discard pile, or offered by the pending decision. Not for cards in the power decks." )]
	public static string GetCardDetails(
		[Description( "The card's name, e.g. 'Shatter Homesteads'. An option text such as 'Shatter Homesteads $2 (Slow)' is also accepted." )] string name,
		[Description( GameIdDescription )] string? gameId = null )
		=> GameStore.Use( gameId, g => {
			g.GetDecision(); // settle on the pending decision so its offered cards are included
			ActionScope.Initialize( g.GameState.RootScope ); // engine state is read through AsyncLocal scope
			var known = g.KnownCards().ToList();
			var matches = known.Where( k => string.Equals( k.Card.Title, name, StringComparison.OrdinalIgnoreCase )
				|| string.Equals( ((IOption)k.Card).Text, name, StringComparison.OrdinalIgnoreCase ) ).ToList();
			if( matches.Count == 0 )
				throw new McpException( $"No card named '{name}' in your hand, in play, discard pile or the pending decision. Known cards: {string.Join( ", ", known.Select( k => k.Card.Title ).Distinct() )}." );
			// The same card can appear in several places (e.g. offered while also in the discard); report each place.
			JsonObject json = PlayerObservation.CardJson( matches[0].Card );
			json["locations"] = new JsonArray( [.. matches.Select( m => (JsonNode)m.Location ).Distinct()] );
			return json.ToJsonString( Pretty );
		} );

	[McpServerTool( Name = "predict_invader_phase", ReadOnly = true )]
	[Description( "Predicts the coming Invader Phase by playing it out on a copy of the game (the real game is not changed): the Ravage, Build and Explore of the three visible Invader cards, from the current state. "
		+ "Returns the cards, the outcome (null, or a win/loss, or where the blight card would flip), blight on the card and fear before -> after, presence destroyed, each land's token changes, the choices the simulation made for you (first option, e.g. where blight cascades - you choose for real), the Invader log, and what is NOT included. "
		+ "Hidden information stays hidden: earned (face-down) fear cards are not resolved, and an unflipped blight card's effect is not run or named. "
		+ "Call it before deciding which lands to protect; call it again after a Fast power changes the board. During the Slow phase it predicts next round's Invader Phase." )]
	public static string PredictInvaderPhase(
		[Description( GameIdDescription )] string? gameId = null )
		=> GameStore.Use( gameId, g => {
			if( g.GetDecision() is null ) throw new McpException( "The game is over." );
			try { return InvaderPhasePrediction.Predict( g ).ToJsonString( Pretty ); }
			catch( InvalidOperationException ex ) { throw new McpException( ex.Message ); }
		} );

	[McpServerTool( Name = "get_power_targets", ReadOnly = true )]
	[Description( "Returns the lands each of your powers could target right now, worked out by the engine's own rules (range, from presence or sacred site, target filter, your spirit's special rules), e.g. {\"targets\":{\"Shatter Homesteads\":[\"A1\",\"A5\"],\"Lightning's Boon\":\"a Spirit (yourself)\"}}. "
		+ "With no name: every card in hand and in play, and every innate. An empty list means the power has no legal target now (e.g. a sacred-site range with no sacred site). "
		+ "Based on the current board, presence and elements: placing presence, or gaining elements that extend a range, changes the answer. Worked out on a copy of the game; if a range bonus asked an element-threshold question, 'choicesMade' says what was assumed." )]
	public static string GetPowerTargets(
		[Description( "Optional: one card's or innate's name (a card's option text also works). Leave out for all of them." )] string? name = null,
		[Description( GameIdDescription )] string? gameId = null )
		=> GameStore.Use( gameId, g => {
			if( g.GetDecision() is null ) throw new McpException( "The game is over." );
			try { return PowerTargets.Get( g, name ).ToJsonString( Pretty ); }
			catch( ArgumentException ex ) { throw new McpException( ex.Message ); }
		} );

	[McpServerTool( Name = "select_action" )]
	[Description( "Chooses one option of the pending decision and advances the game to the next decision. Rejected if the version is stale or the option text isn't offered." )]
	public static string SelectAction(
		[Description( "The version from the decision you are answering" )] int version,
		[Description( "Exact text of the chosen option" )] string option,
		[Description( GameIdDescription )] string? gameId = null )
		=> GameStore.Use( gameId, g => {
			try { g.Select( version, option ); }
			catch( Exception ex ) when( ex is ArgumentException or InvalidOperationException ) {
				throw new McpException( ex.Message ); // the SDK hides other exception messages from the client
			}
			return DecisionNode( g ).ToJsonString( Pretty );
		} );

	[McpServerTool( Name = "select_actions" )]
	[Description( "Chooses several options in a row, starting with the pending decision, and returns the decision reached (same shape as select_action) plus 'batch': {applied, requested, stoppedBecause}. "
		+ "Stops early, without error, at the first step whose option isn't offered or whose 'prompt' doesn't match the start of the actual prompt, so an unexpected prompt (e.g. a fear card's) is never answered by mistake: look at the returned decision and continue. "
		+ "Give 'prompt' whenever the same option text could answer another question (e.g. a land name). A step naming an option the server just auto-selected counts as done. "
		+ "The returned alerts cover every decision passed through, with one 'batch' alert per step naming the prompt it answered." )]
	public static string SelectActions(
		[Description( "The version from the decision the first step answers" )] int version,
		[Description( "Steps in order. Each: {\"option\": exact option text, \"prompt\": optional start of the prompt it is meant to answer, e.g. \"Shatter Homesteads: Target Space\"}" )] BatchStep[] actions,
		[Description( GameIdDescription )] string? gameId = null )
		=> GameStore.Use( gameId, g => {
			BatchResult result;
			try { result = g.SelectBatch( version, actions ); }
			catch( Exception ex ) when( ex is ArgumentException or InvalidOperationException ) {
				throw new McpException( ex.Message );
			}
			JsonObject json = DecisionNode( g );
			json["batch"] = new JsonObject {
				["applied"] = result.Applied,
				["requested"] = result.Requested,
				["stoppedBecause"] = result.StoppedBecause,
			};
			return json.ToJsonString( Pretty );
		} );

	[McpServerTool( Name = "get_game_result", ReadOnly = true )]
	[Description( "Returns whether the game is over and, if so, the outcome (Victory/Defeat), cause, rounds played and number of decisions made." )]
	public static string GetGameResult(
		[Description( GameIdDescription )] string? gameId = null )
		=> GameStore.Use( gameId, g => {
			g.GetDecision(); // let the engine settle on its next decision or end
			var r = g.Result;
			return new JsonObject {
				["over"] = g.IsOver,
				["result"] = r?.Result.ToString(),
				["cause"] = r?.Cause,
				["error"] = g.Error,
				["round"] = g.GameState.RoundNumber,
				["decisionsMade"] = g.History.Count,
			}.ToJsonString( Pretty );
		} );

	static JsonObject DecisionNode( AgentGame g ) {
		DecisionSnapshot? d = g.GetDecision();
		if( d is null )
			return new JsonObject {
				["gameOver"] = true,
				["result"] = g.Result?.ToString() ?? g.Error,
				["alerts"] = AlertsJson( g.FinalAlerts ),
			};
		return new JsonObject {
			["version"] = d.Version,
			["prompt"] = d.Prompt,
			["options"] = new JsonArray( [.. d.Options.Select( o => (JsonNode)o )] ),
			["alerts"] = AlertsJson( d.Alerts ),
			["promptDetails"] = d.PromptDetails,
			["optionDetails"] = new JsonObject( d.OptionDetails.Select( kv => KeyValuePair.Create<string, JsonNode?>( kv.Key, kv.Value.DeepClone() ) ) ),
		};
	}

	static JsonArray AlertsJson( IEnumerable<Alert> alerts )
		=> new( [.. alerts.Select( a => (JsonNode)new JsonObject { ["kind"] = a.Kind, ["text"] = a.Text } )] );
}
