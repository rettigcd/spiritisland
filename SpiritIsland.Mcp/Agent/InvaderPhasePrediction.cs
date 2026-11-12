using System.Text.Json.Nodes;

namespace SpiritIsland.Agent;

/// <summary>
/// Predicts the coming Invader Phase by playing it out on a copy of the game (see <see cref="GameCopy"/>). The live game is never touched.
///
/// Only information a player has is used or reported:
/// - Earned fear cards are face down until the Invader Phase flips them, so they are NOT resolved; the report says how many will.
/// - An unflipped blight card is swapped for a stand-in. If blight on the card would run out, the simulation stops there and
///   says the card would flip, without running (or naming) the real card's effect.
/// - Only the three visible Invader cards act. The deck is never advanced, so no new card is revealed.
/// - As a last safety net, the names of hidden fear cards and of an unflipped blight card are redacted from all text.
/// Decisions the simulation meets (e.g. where blight cascades) are answered with the first option and listed.
/// </summary>
public static class InvaderPhasePrediction {

	public static JsonObject Predict( AgentGame game ) {
		GameState live = game.GameState;
		ActionScope.Initialize( live.RootScope );
		Phase phase = live.Phase;
		if( phase == Phase.Invaders )
			throw new InvalidOperationException( "The Invader Phase is already under way; its results arrive as alerts." );
		string[] hidden = HiddenTexts( live );

		var sim = new Simulation( phase );
		sim.Run( game );
		ActionScope.Initialize( live.RootScope ); // back to the live game for this thread

		var notIncluded = new JsonArray();
		if( phase == Phase.Slow )
			notIncluded.Add( "This round's Invader Phase is over, so this is next round's: time passing was simulated first (damage healed, end-of-turn effects removed), but not next round's growth or powers." );
		else
			notIncluded.Add( "Powers you haven't resolved yet (the rest of this round's Fast powers) - it shows what happens if nothing else changes first." );
		if( sim.FearCardsPendingBefore > 0 )
			notIncluded.Add( $"{sim.FearCardsPendingBefore} earned fear card(s) will resolve at the start of the Invader Phase, before the Invaders act. They are face down, so their effects are not simulated." );

		var result = new JsonObject {
			["invaderCards"] = sim.InvaderCards,
			["outcome"] = sim.Outcome, // null: the phase completes with no win or loss
			["blightOnCard"] = $"{sim.BlightBefore} -> {sim.BlightAfter}",
			["fearPool"] = $"{sim.FearBefore} -> {sim.FearAfter}",
			["fearCardsEarned"] = sim.FearCardsPendingAfter - sim.FearCardsPendingBefore,
			["presenceDestroyed"] = sim.PresenceDestroyed,
			["lands"] = sim.LandChanges, // only lands that change
			["choicesMade"] = new JsonArray( [.. sim.Choices.Select( c => (JsonNode)c )] ),
			["log"] = new JsonArray( [.. sim.Log.Select( a => (JsonNode)a )] ),
			["notIncluded"] = notIncluded,
		};
		return Redact( result, hidden );
	}

	/// <summary> Names a player must not learn: face-down fear cards, and the blight card until it flips. </summary>
	static string[] HiddenTexts( GameState gs ) {
		var texts = gs.Fear.Deck.Concat( gs.Fear.ActivatedCards ).Select( c => ((IOption)c).Text ).ToList();
		if( !gs.BlightCard.CardFlipped )
			texts.AddRange( [gs.BlightCard.Name, gs.BlightCard.Description] );
		return [.. texts.Where( t => !string.IsNullOrWhiteSpace( t ) ).Distinct()];
	}

	static JsonObject Redact( JsonObject json, string[] hidden ) {
		string text = json.ToJsonString();
		foreach( string h in hidden ) {
			string escaped = System.Text.Json.JsonSerializer.Serialize( h )[1..^1]; // match it as it appears inside JSON strings
			text = text.Replace( escaped, "[hidden]" );
		}
		return JsonNode.Parse( text )!.AsObject();
	}

	sealed class Simulation( Phase livePhase ) {

		public JsonObject InvaderCards = [];
		public string? Outcome;
		public int BlightBefore, BlightAfter, FearBefore, FearAfter, FearCardsPendingBefore, FearCardsPendingAfter, PresenceDestroyed;
		public JsonObject LandChanges = [];
		public List<string> Choices = [];
		public List<string> Log = [];

		public void Run( AgentGame game ) {
			var log = new AlertBuffer();
			GameCopy.Result run = GameCopy.Run( game, async gs => {
				if( !gs.BlightCard.CardFlipped )
					gs.BlightCard = new StandInBlightCard(); // the blight count lives on the island's tokens, so it carries over
				gs.NewLogEntry += log.Add;

				if( livePhase == Phase.Slow )
					await gs.TriggerTimePasses();
				log.Drain(); // only the Invader Phase itself is reported

				Before( gs );
				gs.Phase = Phase.Invaders;
				await gs.RunPreInvaderActions(); // public: e.g. a flipped Blighted Island card's effect
				// Not gs.Fear.ResolveActivatedCards(): those cards are face down.
				gs.CheckWinLoss();
				foreach( InvaderSlot slot in gs.InvaderDeck.ActiveSlots ) {
					await slot.Execute( gs );
					gs.CheckWinLoss();
				}
				// Not gs.InvaderDeck.AdvanceAsync(): that would reveal the next Invader card.
			} );

			Choices = run.Choices;
			Outcome = run.Error switch {
				null => run.TimedOut ? "Simulation stopped: it took too long." : null,
				GameOverException over => over.Status.ToString(),
				StandInBlightCard.FlipException => "Blight on the card runs out: the blight card would flip here. Its effect is hidden until it really flips, so the simulation stops at this point.",
				var ex => $"Simulation failed: {ex.Message}",
			};
			if( run.Copy is not null )
				After( run.Copy );
			Log = [.. log.Drain().Where( a => a.Kind is "invaders" or "bad" or "fear-blight" ).Select( a => a.Text )];
		}

		Dictionary<string, Dictionary<string, int>> _tokensBefore = [];
		int _destroyedBefore;

		void Before( GameState gs ) {
			InvaderDeck deck = gs.InvaderDeck;
			InvaderCards = new JsonObject {
				["ravage"] = Cards( deck.Ravage.Cards ),
				["build"] = Cards( deck.Build.Cards ),
				["explore"] = Cards( deck.Explore.Cards ),
			};
			BlightBefore = gs.BlightCard.BlightCount;
			FearBefore = gs.Fear.EarnedFear;
			FearCardsPendingBefore = gs.Fear.ActivatedCards.Count;
			_destroyedBefore = gs.Spirits.Single().Presence.Destroyed.Count;
			_tokensBefore = gs.Spaces_Existing.ToDictionary( s => s.SpaceSpec.Label, Tokens );
			static JsonArray Cards( IEnumerable<InvaderCard> cards ) => new( [.. cards.Select( c => (JsonNode)((IOption)c).Text )] );
		}

		void After( GameState gs ) {
			BlightAfter = gs.BlightCard.BlightCount;
			FearAfter = gs.Fear.EarnedFear;
			FearCardsPendingAfter = gs.Fear.ActivatedCards.Count;
			PresenceDestroyed = gs.Spirits.Single().Presence.Destroyed.Count - _destroyedBefore;
			foreach( Space space in gs.Spaces_Existing ) {
				string label = space.SpaceSpec.Label;
				Dictionary<string, int> before = _tokensBefore.GetValueOrDefault( label ) ?? [];
				Dictionary<string, int> after = Tokens( space );
				var changes = before.Keys.Union( after.Keys ).Order()
					.Select( k => (k, b: before.GetValueOrDefault( k ), a: after.GetValueOrDefault( k )) )
					.Where( x => x.b != x.a )
					.Select( x => $"{x.k} {x.b}->{x.a}" ).ToArray();
				if( changes.Length > 0 )
					LandChanges[label] = string.Join( ", ", changes );
			}
		}

		static Dictionary<string, int> Tokens( Space space ) {
			var tokens = new Dictionary<string, int>();
			foreach( IToken t in space.OfType<IToken>() )
				tokens[t.Text] = tokens.GetValueOrDefault( t.Text ) + space[t];
			return tokens;
		}
	}

	/// <summary> Stands in for an unflipped blight card: stops the simulation where the real card would flip, without revealing it. </summary>
	sealed class StandInBlightCard() : BlightCard( "the blight card", "hidden until it flips", 0 ) {
		public sealed class FlipException : Exception;
		public override IActOn<GameState> Immediately => new BaseCmd<GameState>( "hidden", (Action<GameState>)(_ => throw new FlipException()) );
	}
}
