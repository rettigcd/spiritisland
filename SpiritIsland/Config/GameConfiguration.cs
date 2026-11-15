#nullable enable
namespace SpiritIsland;

public class GameConfiguration {

	public string[]? Spirits;
	public AspectConfigKey[]? Aspects;
	public string[]? Boards;
	public int ShuffleNumber;
	public AdversaryConfig? Adversary;
	public bool CommandTheBeasts;

	public string AdversarySummary => Adversary == null ? "[none]" : $"{Adversary.Name} {Adversary.Level}";

	#region public helper methods
	public GameConfiguration ConfigSpirits(params string[] spirits ) { Spirits = spirits; return this; }
	public GameConfiguration ConfigAspects(params AspectConfigKey[] aspects) { Aspects = aspects; return this; }
	public GameConfiguration ConfigBoards( params string[] boards ) { Boards = boards; return this; }
	public GameConfiguration ConfigCommandBeasts( bool cmdBeasts ) { CommandTheBeasts = cmdBeasts; return this; }
	public GameConfiguration ConfigAdversary(string adversaryName, int level) { return ConfigAdversary(new AdversaryConfig(adversaryName, level)); }
	public GameConfiguration ConfigAdversary( AdversaryConfig adversary ) {  Adversary = adversary; return this; }
	#endregion

	#region Json

	/// <summary>
	/// Everything a GameBuilder needs to rebuild the same starting game - what SoloGame.Load requires as its
	/// precondition. Named keys (it is the outermost thing a person opens in a save file); null/empty
	/// sections are omitted.
	/// </summary>
	public JsonObject ToJson() {
		var json = new JsonObject { ["ShuffleNumber"] = ShuffleNumber, ["CommandTheBeasts"] = CommandTheBeasts };
		if( Spirits is not null ) json["Spirits"] = new JsonArray( Spirits.Select( s => (JsonNode)JsonValue.Create( s )! ).ToArray() );
		if( Aspects is not null ) json["Aspects"] = new JsonArray( Aspects.Select( a => (JsonNode)new JsonArray( a.Spirit, a.Aspect ) ).ToArray() );
		if( Boards is not null ) json["Boards"] = new JsonArray( Boards.Select( b => (JsonNode)JsonValue.Create( b )! ).ToArray() );
		if( Adversary is not null ) json["Adversary"] = Adversary.ToJson();
		return json;
	}

	public static GameConfiguration FromJson( JsonObject json ) => new() {
		ShuffleNumber = json["ShuffleNumber"]!.GetValue<int>(),
		CommandTheBeasts = json["CommandTheBeasts"]!.GetValue<bool>(),
		Spirits = json["Spirits"] is JsonArray spirits ? [.. spirits.Select( n => n!.GetValue<string>() )] : null,
		Aspects = json["Aspects"] is JsonArray aspects ? [.. aspects.Select( n => new AspectConfigKey( n![0]!.GetValue<string>(), n[1]!.GetValue<string>() ) )] : null,
		Boards = json["Boards"] is JsonArray boards ? [.. boards.Select( n => n!.GetValue<string>() )] : null,
		Adversary = json["Adversary"] is JsonArray adversary ? AdversaryConfig.FromJson( adversary ) : null,
	};

	#endregion
}
