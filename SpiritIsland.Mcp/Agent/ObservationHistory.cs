using System.Text.Json.Nodes;

namespace SpiritIsland.Agent;

/// <summary>
/// Remembers the last few observations sent, so the next one can be sent as just what changed since one of them.
/// Diffs are against what the agent was actually sent (not against a decision's state), so no per-decision history is needed.
/// Any id not remembered (too old, unknown, from another game) gets the full observation instead.
/// </summary>
public sealed class ObservationHistory( int keep = 3 ) {

	/// <summary>
	/// Returns the observation with an "observationId". If <paramref name="sinceId"/> is one of the remembered ones, only what
	/// changed since it is returned, under "changes" (see <see cref="Diff"/>); otherwise the full observation, with "full": true.
	/// </summary>
	public JsonObject Next( JsonObject observation, int? sinceId ) {
		int id = ++_lastId;
		JsonObject? basis = sinceId is int s ? _sent.FirstOrDefault( x => x.Id == s ).Observation : null;

		_sent.Add( (id, (JsonObject)observation.DeepClone()) );
		if( _sent.Count > keep ) _sent.RemoveAt( 0 );

		if( basis is null ) {
			var full = new JsonObject { ["observationId"] = id, ["full"] = true };
			foreach( var kv in observation.ToList() ) {
				observation.Remove( kv.Key );
				full[kv.Key] = kv.Value;
			}
			return full;
		}
		return new JsonObject {
			["observationId"] = id,
			["since"] = sinceId,
			["changes"] = Diff( basis, observation ),
		};
	}

	/// <summary>
	/// What changed from <paramref name="before"/> to <paramref name="after"/>: keys left out are unchanged, a key set to null
	/// was removed, objects are compared key by key, and anything else (arrays, values) is sent whole if it changed.
	/// Each land under "spaces" is also sent whole (its full token list) if it changed, or null if it no longer has tokens.
	/// </summary>
	public static JsonObject Diff( JsonObject before, JsonObject after, bool wholeChildren = false ) {
		var diff = new JsonObject();
		foreach( var (key, value) in after ) {
			JsonNode? old = before[key];
			if( !before.ContainsKey( key ) )
				diff[key] = value?.DeepClone();
			else if( !wholeChildren && old is JsonObject o && value is JsonObject n ) {
				JsonObject inner = Diff( o, n, wholeChildren: key == "spaces" );
				if( inner.Count > 0 ) diff[key] = inner;
			} else if( !JsonNode.DeepEquals( old, value ) )
				diff[key] = value?.DeepClone();
		}
		foreach( var (key, _) in before )
			if( !after.ContainsKey( key ) )
				diff[key] = null;
		return diff;
	}

	/// <summary> Applies a <see cref="Diff"/> to the observation it was made from (what an agent does in its head). </summary>
	public static JsonObject Apply( JsonObject before, JsonObject diff, bool wholeChildren = false ) {
		var result = (JsonObject)before.DeepClone();
		foreach( var (key, value) in diff ) {
			if( value is null ) result.Remove( key );
			else if( !wholeChildren && value is JsonObject d && result[key] is JsonObject o )
				result[key] = Apply( o, d, wholeChildren: key == "spaces" );
			else result[key] = value.DeepClone();
		}
		return result;
	}

	public JsonObject ToJson() => new() {
		["LastId"] = _lastId,
		["Sent"] = new JsonArray( [.. _sent.Select( x => (JsonNode)new JsonObject { ["Id"] = x.Id, ["Observation"] = x.Observation.DeepClone() } )] ),
	};

	public void Restore( JsonObject json ) {
		_lastId = json["LastId"]!.GetValue<int>();
		_sent.Clear();
		_sent.AddRange( json["Sent"]!.AsArray().Select( n => (n!["Id"]!.GetValue<int>(), (JsonObject)n["Observation"]!.DeepClone()) ) );
	}

	readonly List<(int Id, JsonObject Observation)> _sent = [];
	int _lastId;
}
