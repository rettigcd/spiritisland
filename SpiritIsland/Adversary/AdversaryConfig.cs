namespace SpiritIsland;

public record AdversaryConfig(string Name, int Level) {
	public static readonly AdversaryConfig NullAdversary = new AdversaryConfig("", 0);

	/// <summary>
	/// Level/escalation is static config, fixed once at Adversary construction and never reassigned
	/// anywhere in the engine (see docs/GameSerialization-Roadmap.md section 9) - so {Name, Level}
	/// alone fully determines an adversary's identity, no separate "progress" to capture.
	/// </summary>
	public JsonArray ToJson() => new JsonArray( Name, Level );

	public static AdversaryConfig FromJson( JsonArray json ) => new( json[0]!.GetValue<string>(), json[1]!.GetValue<int>() );
}
