using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using SpiritIsland.Agent;

namespace SpiritIsland.Mcp;

/// <summary>
/// MCP adapter over <see cref="AgentGame"/>. Holds a single game at a time: creating a new game abandons the
/// previous one (the engine isn't known to be safe for concurrent games).
/// </summary>
[McpServerToolType]
public sealed class GameTools {

	static readonly JsonSerializerOptions Pretty = new() { WriteIndented = false };

	[McpServerTool( Name = "get_game_configuration_options", ReadOnly = true )]
	[Description( "Lists the spirits, boards and adversaries (with their valid levels; 0 = escalation only) that create_game accepts." )]
	public static string GetGameConfigurationOptions() => new JsonObject {
		["spirits"] = new JsonArray( [.. GameSetup.Spirits.Select( s => (JsonNode)s )] ),
		["boards"] = new JsonArray( [.. GameSetup.Boards.Select( s => (JsonNode)s )] ),
		["adversaries"] = new JsonObject( GameSetup.Adversaries.Select( kv =>
			KeyValuePair.Create<string, JsonNode?>( kv.Key, new JsonArray( [.. kv.Value.Select( l => (JsonNode)l )] ) ) ) ),
	}.ToJsonString( Pretty );

	[McpServerTool( Name = "create_game" )]
	[Description( "Starts a new single-spirit game (replacing any game in progress) and returns the first decision. Same spirit+board+adversary+seed gives the same setup." )]
	public static string CreateGame(
		[Description( "Spirit name, as listed by get_game_configuration_options" )] string spirit,
		[Description( "Board letter, e.g. A" )] string board,
		[Description( "Integer random seed" )] int seed,
		[Description( "Optional adversary name" )] string? adversary = null,
		[Description( "Adversary level (default 0)" )] int adversaryLevel = 0 ) {
		lock( _lock ) {
			try { _game = new AgentGame( new NewGameRequest( spirit, board, seed, adversary, adversaryLevel ) ); }
			catch( ArgumentException ex ) { throw new McpException( ex.Message ); }
			return DecisionJson();
		}
	}

	[McpServerTool( Name = "get_observation", ReadOnly = true )]
	[Description( "Returns everything a human player could see right now: island, invaders (revealed cards only), fear, blight, and your spirit. Hidden deck contents are never included." )]
	public static string GetObservation() {
		lock( _lock )
			return PlayerObservation.Build( Game ).ToJsonString( Pretty );
	}

	[McpServerTool( Name = "get_decision", ReadOnly = true )]
	[Description( "Returns the pending decision: a prompt, a version, and the list of legal options. An option's text is its ID." )]
	public static string GetDecision() {
		lock( _lock )
			return DecisionJson();
	}

	[McpServerTool( Name = "select_action" )]
	[Description( "Chooses one option of the pending decision and advances the game to the next decision. Rejected if the version is stale or the option text isn't offered." )]
	public static string SelectAction(
		[Description( "The version from the decision you are answering" )] int version,
		[Description( "Exact text of the chosen option" )] string option ) {
		lock( _lock ) {
			try { Game.Select( version, option ); }
			catch( Exception ex ) when( ex is ArgumentException or InvalidOperationException ) {
				throw new McpException( ex.Message ); // the SDK hides other exception messages from the client
			}
			return DecisionJson();
		}
	}

	[McpServerTool( Name = "get_game_result", ReadOnly = true )]
	[Description( "Returns whether the game is over and, if so, the outcome (Victory/Defeat), cause, rounds played and number of decisions made." )]
	public static string GetGameResult() {
		lock( _lock ) {
			AgentGame g = Game;
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
		}
	}

	static string DecisionJson() {
		AgentGame g = Game;
		DecisionSnapshot? d = g.GetDecision();
		if( d is null )
			return new JsonObject { ["gameOver"] = true, ["result"] = g.Result?.ToString() ?? g.Error }.ToJsonString( Pretty );
		return new JsonObject {
			["version"] = d.Version,
			["prompt"] = d.Prompt,
			["options"] = new JsonArray( [.. d.Options.Select( o => (JsonNode)o )] ),
		}.ToJsonString( Pretty );
	}

	static AgentGame Game => _game ?? throw new McpException( "No game in progress. Call create_game first." );

	static readonly object _lock = new();
	static AgentGame? _game;
}
