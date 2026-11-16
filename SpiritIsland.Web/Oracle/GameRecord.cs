using System.Text.Json.Nodes;
using SpiritIsland.SinglePlayer;

namespace SpiritIsland.Web.Oracle;

/// <summary>
/// What the server keeps for one game: the configuration that rebuilds it and the newest action-boundary checkpoint.
/// Nothing about an action in progress is kept - those picks live in the browser (see docs/WebClient-Plan.md).
/// Written as a <see cref="SoloGameFile"/> with the extra fields below beside it.
/// </summary>
/// <param name="CheckpointVersion">Bumped each time the checkpoint is replaced; a request made against an older one is stale.</param>
/// <param name="FinishingPicks">Only when <paramref name="Over"/>: the picks after the checkpoint that ended the game, so its final state can be rebuilt.</param>
public sealed record GameRecord(
	string Id,
	GameConfiguration Config,
	int CheckpointVersion,
	JsonObject Checkpoint,
	bool Over = false,
	string[]? FinishingPicks = null
) {

	public JsonObject ToJson() {
		JsonObject json = SoloGameFile.ToJson( Config, (JsonObject)Checkpoint.DeepClone() );
		json["Id"] = Id;
		json["CheckpointVersion"] = CheckpointVersion;
		json["Over"] = Over;
		json["FinishingPicks"] = new JsonArray( [.. (FinishingPicks ?? []).Select( p => (JsonNode)p )] );
		return json;
	}

	public static GameRecord FromJson( JsonObject json ) => new(
		json["Id"]!.GetValue<string>(),
		GameConfiguration.FromJson( (JsonObject)json["Config"]! ),
		json["CheckpointVersion"]!.GetValue<int>(),
		(JsonObject)json["Game"]!.DeepClone(),
		json["Over"]!.GetValue<bool>(),
		[.. json["FinishingPicks"]!.AsArray().Select( n => n!.GetValue<string>() )]
	);
}
