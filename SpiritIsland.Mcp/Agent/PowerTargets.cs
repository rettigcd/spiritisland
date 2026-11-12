using System.Text.Json.Nodes;

namespace SpiritIsland.Agent;

/// <summary>
/// Which lands each of the spirit's powers could target right now, worked out by the engine's own targeting (range, from
/// presence or sacred site, target filter, spirit mods) on a copy of the game (see <see cref="GameCopy"/>), so a range bonus
/// that asks an element-threshold question can't put a prompt on the real game.
/// </summary>
public static class PowerTargets {

	/// <param name="name">A card's or innate's name (or a card's option text); null for every card in hand and in play, and every innate.</param>
	/// <exception cref="ArgumentException">no such power</exception>
	public static JsonObject Get( AgentGame game, string? name ) {
		GameState live = game.GameState;
		ActionScope.Initialize( live.RootScope );
		Spirit spirit = live.Spirits.Single();

		string[] titles = name is null
			? [.. spirit.Hand.Concat( spirit.InPlay ).Select( c => c.Title ).Concat( spirit.InnatePowers.Select( p => p.Title ) ).Distinct()]
			: [Find( spirit, name )];

		var targets = new Dictionary<string, JsonNode?>();
		GameCopy.Result run = GameCopy.Run( game, async gs => {
			Spirit s = gs.Spirits.Single();
			foreach( string title in titles ) {
				Task<Space[]?> options = s.Hand.Concat( s.InPlay ).Concat( s.DiscardPile ).FirstOrDefault( c => c.Title == title ) is { } card
					? card.GetTargetSpaceOptions( s )
					: s.InnatePowers.First( p => p.Title == title ).GetTargetSpaceOptions( s );
				Space[]? lands = await options;
				targets[title] = lands is null ? "a Spirit (yourself)" : new JsonArray( [.. lands.Select( l => (JsonNode)l.SpaceSpec.Label )] );
			}
		} );
		ActionScope.Initialize( live.RootScope ); // back to the live game for this thread

		var result = new JsonObject {
			["targets"] = new JsonObject( titles.Select( t => KeyValuePair.Create( t, targets.GetValueOrDefault( t )?.DeepClone() ) ) ),
		};
		if( run.Choices.Count > 0 )
			result["choicesMade"] = new JsonArray( [.. run.Choices.Select( c => (JsonNode)c )] );
		if( run.Error is not null || run.TimedOut )
			result["error"] = run.TimedOut ? "Working out targets took too long." : run.Error!.Message;
		return result;
	}

	static string Find( Spirit spirit, string name ) {
		IEnumerable<string> cards = spirit.Hand.Concat( spirit.InPlay ).Concat( spirit.DiscardPile )
			.Where( c => string.Equals( c.Title, name, StringComparison.OrdinalIgnoreCase )
				|| string.Equals( ((IOption)c).Text, name, StringComparison.OrdinalIgnoreCase ) )
			.Select( c => c.Title );
		IEnumerable<string> innates = spirit.InnatePowers
			.Where( p => string.Equals( p.Title, name, StringComparison.OrdinalIgnoreCase ) )
			.Select( p => p.Title );
		return cards.Concat( innates ).FirstOrDefault()
			?? throw new ArgumentException( $"No power named '{name}' in your hand, in play, discard pile or innate powers." );
	}
}
