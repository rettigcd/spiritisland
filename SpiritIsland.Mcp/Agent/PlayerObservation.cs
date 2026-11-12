using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;

namespace SpiritIsland.Agent;

/// <summary>
/// Projects the live GameState into the information a human player legitimately has, split in two:
/// <see cref="BuildStatic"/> - what rarely changes (layout, spirit rules, track layouts, legend), sent once per game;
/// <see cref="Build"/> - what changes as the game is played. The observation carries the static info's
/// <c>staticVersion</c>, so an agent can tell when to fetch the static info again (e.g. after the island's layout changes).
/// Hidden by construction (never read here): invader-deck order/contents, power-card deck order,
/// face-down fear cards, unused invader cards, the unflipped blight card. Only counts of those are reported.
/// </summary>
public static class PlayerObservation {

	#region dynamic

	public static JsonObject Build( AgentGame game ) {
		GameState gs = game.GameState;
		ActionScope.Initialize( gs.RootScope ); // engine state is read through AsyncLocal scope
		Spirit spirit = gs.Spirits.Single();

		return new JsonObject {
			["staticVersion"] = StaticVersion( game ),
			["round"] = gs.RoundNumber,
			["phase"] = gs.Phase.ToString(),
			["fear"] = FearJson( gs ),
			["blight"] = BlightJson( gs ),
			["invaders"] = InvadersJson( gs ),
			["spirit"] = SpiritJson( spirit ),
			// Lands with no tokens are left out.
			["spaces"] = new JsonObject( gs.Spaces_Existing
				.Select( s => KeyValuePair.Create<string, JsonNode?>( s.SpaceSpec.Label, TokensJson( s ) ) )
				.Where( kv => kv.Value!.AsObject().Count > 0 ) ),
		};
	}

	static JsonObject FearJson( GameState gs ) => new() {
		["terrorLevel"] = gs.Fear.TerrorLevel, // its victory condition is in the static info
		["fearPool"] = gs.Fear.EarnedFear,
		["fearPoolMax"] = gs.Fear.PoolMax,
		["facedownCardsRemaining"] = gs.Fear.Deck.Count, // count only; identities are hidden
		// Earned cards lie face down until the next Invader Phase flips them, so only how many are waiting is public.
		// Each card's name and effect arrive as an alert when it resolves.
		["earnedFearCardsPending"] = gs.Fear.ActivatedCards.Count,
	};

	static JsonObject BlightJson( GameState gs ) {
		BlightCard card = gs.BlightCard;
		// The card is set up Healthy side up without looking at the back, so its name and Blighted Island effect are
		// hidden until it flips. Only the blight count (printed on the front) is public before then.
		return new() {
			["cardFlipped"] = card.CardFlipped,
			["card"] = card.CardFlipped ? card.Name : null,
			["blightedIslandEffect"] = card.CardFlipped ? card.Description : null,
			["blightRemainingOnCard"] = (int)card.Memento, // only public accessor of the count
			["blightCardsRemaining"] = gs.BlightCards.Count,
		};
	}

	static JsonObject InvadersJson( GameState gs ) {
		InvaderDeck deck = gs.InvaderDeck;
		return new JsonObject {
			["explore"] = Cards( deck.Explore.Cards ),
			["build"] = Cards( deck.Build.Cards ),
			["ravage"] = Cards( deck.Ravage.Cards ),
			["discards"] = Cards( deck.Discards ),
			["unrevealedCount"] = deck.UnrevealedCards.Count, // order/contents hidden
			// Lands whose terrain the card in each slot matches (what that action looks at). Not a prediction of what the action will do,
			// and an adversary or power can still change it.
			["matchedLands"] = new JsonObject {
				["explore"] = MatchedLands( deck.Explore.Cards ),
				["build"] = MatchedLands( deck.Build.Cards ),
				["ravage"] = MatchedLands( deck.Ravage.Cards ),
			},
		};
		JsonArray MatchedLands( IEnumerable<InvaderCard> cards ) => new( [.. gs.Spaces_Existing
			.Where( space => cards.Any( card => card.MatchesCard( space ) ) )
			.Select( space => (JsonNode)space.SpaceSpec.Label )] );
		static JsonArray Cards( IEnumerable<InvaderCard> cards ) => new( [.. cards.Select( c => (JsonNode)((IOption)c).Text )] );
	}

	static JsonObject SpiritJson( Spirit s ) => new() {
		["energy"] = s.Energy,
		["elements"] = s.Elements.Summary(),
		["presence"] = new JsonObject {
			// How far along each track (static info lists the slots); a list of revealed slot indexes if not revealed in order.
			["energyTrackRevealed"] = RevealedJson( s.Presence.Energy ),
			["cardPlayTrackRevealed"] = RevealedJson( s.Presence.CardPlays ),
			["energyPerTurn"] = s.Presence.EnergyPerTurn,
			["cardPlaysPerTurn"] = s.Presence.CardPlayPerTurn,
			["destroyed"] = s.Presence.Destroyed.Count,
			["sacredSites"] = new JsonArray( [.. s.Presence.SacredSites.Select( space => (JsonNode)space.SpaceSpec.Label )] ), // lands with 2+ Presence; "ss" ranges are measured from these
		},
		["growthGroupsUsed"] = new JsonArray( [.. s.GrowthTrack.Groups.Where( g => g.Used ).Select( g => (JsonNode)g.ToString() )] ),
		// Name, cost, speed, elements only - full text via get_card_details, or the decision's optionDetails when a card is offered.
		["hand"] = new JsonArray( [.. s.Hand.Select( CardSummaryJson )] ),
		["inPlay"] = new JsonArray( [.. s.InPlay.Select( CardSummaryJson )] ),
		["discard"] = new JsonArray( [.. s.DiscardPile.Select( CardSummaryJson )] ),
		// Innate name -> each tier's status against the elements gained so far this turn (tiers are in the static info).
		["innateTierStatus"] = new JsonObject( s.InnatePowers.Select( p => KeyValuePair.Create<string, JsonNode?>( p.Title,
			new JsonArray( [.. p.DrawableOptions.Select( t => (JsonNode)s.Elements.CouldHave( t.Elements ).ToString() )] ) ) ) ),
	};

	static JsonNode RevealedJson( IPresenceTrack t ) {
		var slots = t.Slots.ToList();
		int[] revealed = [.. t.Revealed.Select( r => slots.IndexOf( r ) )];
		bool inOrder = revealed.Select( ( index, i ) => index == i ).All( x => x );
		return inOrder ? revealed.Length : new JsonArray( [.. revealed.Select( i => (JsonNode)i )] );
	}

	static JsonObject TokensJson( Space space ) {
		var tokens = new JsonObject();
		foreach( IToken t in space.OfType<IToken>().OrderBy( t => t.Text ) )
			tokens[t.Text] = space[t];
		return tokens;
	}

	static JsonObject CardSummaryJson( PowerCard c ) => new() {
		["name"] = c.Title, ["cost"] = c.Cost, ["speed"] = c.Speed.ToString(),
		["elements"] = c.Elements.BuildElementString( true ),
	};

	internal static JsonObject CardJson( PowerCard c ) => new() {
		["name"] = c.Title, ["cost"] = c.Cost, ["speed"] = c.Speed.ToString(),
		["elements"] = c.Elements.BuildElementString( true ),
		["range"] = c.RangeText, ["target"] = c.TargetFilter, ["instructions"] = c.Instructions,
	};

	#endregion dynamic

	#region static

	/// <summary>
	/// What rarely changes during a game. Includes <c>staticVersion</c>, a hash of the rest, which changes whenever any of it
	/// does (a board added or removed, lands woven together, a spirit gaining a growth option...).
	/// </summary>
	public static JsonObject BuildStatic( AgentGame game ) {
		JsonObject content = StaticContent( game );
		var result = new JsonObject { ["staticVersion"] = Hash( content ) };
		foreach( var kv in content.ToList() ) {
			content.Remove( kv.Key );
			result[kv.Key] = kv.Value;
		}
		return result;
	}

	public static string StaticVersion( AgentGame game ) => Hash( StaticContent( game ) );

	static string Hash( JsonObject content )
		=> Convert.ToHexString( SHA256.HashData( Encoding.UTF8.GetBytes( content.ToJsonString() ) ) )[..8].ToLowerInvariant();

	static JsonObject StaticContent( AgentGame game ) {
		GameState gs = game.GameState;
		ActionScope.Initialize( gs.RootScope );
		Spirit spirit = gs.Spirits.Single();
		return new JsonObject {
			["setup"] = new JsonObject {
				["spirit"] = spirit.SpiritName,
				["board"] = game.Request.Board,
				["adversary"] = gs.Adversary is { Level: var lvl } adv && adv.Builder is not NullAdversaryBuilder
					? new JsonObject { ["name"] = adv.Name, ["level"] = lvl, ["rules"] = adv.Describe() } : null,
			},
			["legend"] = Legend( spirit ),
			["victoryConditions"] = new JsonObject {
				["terrorLevel1"] = "Win when there are no Invaders on the island",
				["terrorLevel2"] = "Win when there are no Towns or Cities on the island",
				["terrorLevel3"] = "Win when there are no Cities on the island",
				["allFearCardsEarned"] = "Victory",
			},
			["lands"] = new JsonArray( [.. gs.Spaces_Existing.Select( space => (JsonNode)new JsonObject {
				["name"] = space.SpaceSpec.Label,
				["terrain"] = string.Join( "/", Enum.GetValues<Terrain>().Where( t => t != Terrain.None && space.SpaceSpec.Is( t ) ) ),
				["adjacent"] = new JsonArray( [.. space.Adjacent_Existing.Select( a => (JsonNode)a.SpaceSpec.Label )] ),
			} )] ),
			["spirit"] = new JsonObject {
				["name"] = spirit.SpiritName,
				["specialRules"] = new JsonArray( [.. spirit.SpecialRules.Select( r => (JsonNode)new JsonObject {
					["title"] = r.Title, ["description"] = r.Description,
				} )] ),
				["growthGroups"] = new JsonArray( [.. spirit.GrowthTrack.Groups.Select( g => (JsonNode)new JsonObject {
					["group"] = g.ToString(), ["energyChange"] = g.GainEnergy,
				} )] ),
				["presenceTracks"] = new JsonObject {
					["energy"] = SlotsJson( spirit.Presence.Energy ),
					["cardPlay"] = SlotsJson( spirit.Presence.CardPlays ),
				},
				["innatePowers"] = new JsonArray( [.. spirit.InnatePowers.Select( p => (JsonNode)new JsonObject {
					["name"] = p.Title, ["speed"] = p.Speed.ToString(), ["target"] = p.TargetFilter, ["range"] = p.RangeText,
					["instructions"] = p.GeneralInstructions,
					["tiers"] = new JsonArray( [.. p.DrawableOptions.Select( t => (JsonNode)new JsonObject {
						["threshold"] = t.ThresholdString, ["effect"] = t.Description,
					} )] ),
				} )] ),
			},
			// Full text of the cards the spirit started with. Cards gained later: see the draw decision's optionDetails, or get_card_details.
			["startingCards"] = new JsonArray( [.. game.StartingCards.Select( c => (JsonNode)CardJson( c ) )] ),
		};
	}

	static JsonArray SlotsJson( IPresenceTrack t ) => new( [.. t.Slots.Select( x => (JsonNode)((IOption)x).Text )] );

	static JsonObject Legend( Spirit spirit ) => new() {
		["E@n / T@n / C@n"] = "Explorer / Town / City with n health remaining (full: 1 / 2 / 3)",
		["D@n"] = "Dahan with n health remaining (full: 2)",
		["^"] = "after an Invader: one Strife per ^",
		[spirit.Presence.Token.SpaceAbreviation] = "your Presence",
		["other tokens"] = "Blight, Beast, Disease, Wilds, Badlands, Defend (n = amount), Isolate, ...",
		["spaces"] = "the observation lists only lands that have tokens; a land not listed is empty",
		["invader cards"] = "terrain initial (J=Jungle, M=Mountain, S=Sands, W=Wetland), 'Coastal', or two initials joined by + (Stage III); a leading 2 marks a Stage II card that triggers the adversary's Escalation",
		["presence track revealed"] = "number of slots uncovered from the start of the track (an index list if uncovered out of order)",
	};

	#endregion static
}
