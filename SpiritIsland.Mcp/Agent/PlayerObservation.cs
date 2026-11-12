using System.Text.Json.Nodes;

namespace SpiritIsland.Agent;

/// <summary>
/// Projects the live GameState into the information a human player legitimately has.
/// Hidden by construction (never read here): invader-deck order/contents, power-card deck order,
/// face-down fear cards, unused invader cards. Only counts of those are reported.
/// </summary>
public static class PlayerObservation {

	public static JsonObject Build( AgentGame game ) {
		GameState gs = game.GameState;
		ActionScope.Initialize( gs.RootScope ); // engine state is read through AsyncLocal scope
		Spirit spirit = gs.Spirits.Single();

		return new JsonObject {
			["round"] = gs.RoundNumber,
			["phase"] = gs.Phase.ToString(),
			["adversary"] = gs.Adversary is { Level: var lvl } adv && !string.IsNullOrEmpty( adv.Name )
				? new JsonObject { ["name"] = adv.Name, ["level"] = lvl } : null,
			["fear"] = FearJson( gs ),
			["blight"] = BlightJson( gs ),
			["invaders"] = InvadersJson( gs ),
			["spirit"] = SpiritJson( spirit ),
			["spaces"] = new JsonArray( [.. gs.Spaces_Existing.Select( SpaceJson )] ),
		};
	}

	static JsonObject FearJson( GameState gs ) => new() {
		["terrorLevel"] = gs.Fear.TerrorLevel,
		// Mirrors GameState.CheckTerrorLevelVictory (private); the wording needn't match, only the meaning.
		["victoryCondition"] = gs.Fear.TerrorLevel switch {
			1 => "Win when there are no Invaders on the island",
			2 => "Win when there are no Towns or Cities on the island",
			3 => "Win when there are no Cities on the island",
			_ => "Victory (all Fear Cards earned)",
		},
		["fearPool"] = gs.Fear.EarnedFear,
		["fearPoolMax"] = gs.Fear.PoolMax,
		["facedownCardsRemaining"] = gs.Fear.Deck.Count, // count only; identities are hidden
		["revealedCards"] = new JsonArray( [.. gs.Fear.ActivatedCards.Select( c => (JsonNode)c.Text )] ),
	};

	static JsonObject BlightJson( GameState gs ) => new() {
		["card"] = gs.BlightCard.Name,
		["cardFlipped"] = gs.BlightCard.CardFlipped,
		["blightedIslandEffect"] = gs.BlightCard.Description, // what happens when the card flips (the Blighted Island side)
		["blightRemainingOnCard"] = (int)gs.BlightCard.Memento, // only public accessor of the count
		["blightCardsRemaining"] = gs.BlightCards.Count,
	};

	static JsonObject InvadersJson( GameState gs ) {
		InvaderDeck deck = gs.InvaderDeck;
		return new JsonObject {
			["explore"] = Cards( deck.Explore.Cards ),
			["build"] = Cards( deck.Build.Cards ),
			["ravage"] = Cards( deck.Ravage.Cards ),
			["discards"] = Cards( deck.Discards ),
			["unrevealedCount"] = deck.UnrevealedCards.Count, // order/contents hidden			// Lands whose terrain the card in each slot matches (what that action looks at). Not a prediction of what the action will do,
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
		["name"] = s.SpiritName,
		["energy"] = s.Energy,
		["elements"] = s.Elements.Summary(),
		["specialRules"] = new JsonArray( [.. s.SpecialRules.Select( r => (JsonNode)new JsonObject {
			["title"] = r.Title, ["description"] = r.Description,
		} )] ),
		["presence"] = new JsonObject {
			["energyTrack"] = TrackJson( s.Presence.Energy ),
			["cardPlayTrack"] = TrackJson( s.Presence.CardPlays ),
			["energyPerTurn"] = s.Presence.EnergyPerTurn,
			["cardPlaysPerTurn"] = s.Presence.CardPlayPerTurn,
			["destroyed"] = s.Presence.Destroyed.Count,
			["sacredSites"] = new JsonArray( [.. s.Presence.SacredSites.Select( space => (JsonNode)space.SpaceSpec.Label )] ), // lands with 2+ Presence; "ss" ranges are measured from these
		},
		["growth"] = new JsonArray( [.. s.GrowthTrack.Groups.Select( g => (JsonNode)new JsonObject {
			["group"] = g.ToString(), ["used"] = g.Used, ["energyChange"] = g.GainEnergy,
		} )] ),
		["hand"] = new JsonArray( [.. s.Hand.Select( CardJson )] ),
		["inPlay"] = new JsonArray( [.. s.InPlay.Select( CardJson )] ),
		// Compact on purpose - what Reclaim All would return. Full text is available from get_card_details.
		["discard"] = new JsonArray( [.. s.DiscardPile.Select( c => (JsonNode)new JsonObject {
			["name"] = c.Title, ["cost"] = c.Cost, ["speed"] = c.Speed.ToString(),
		} )] ),
		["innatePowers"] = new JsonArray( [.. s.InnatePowers.Select( p => (JsonNode)new JsonObject {
			["name"] = p.Title, ["speed"] = p.Speed.ToString(), ["target"] = p.TargetFilter, ["range"] = p.RangeText,
			["instructions"] = p.GeneralInstructions,
			["tiers"] = new JsonArray( [.. p.DrawableOptions.Select( t => (JsonNode)new JsonObject {
				["threshold"] = t.ThresholdString,
				["effect"] = t.Description,
				["elementStatus"] = s.Elements.CouldHave( t.Elements ).ToString(), // against the elements gained so far this turn
			} )] ),
		} )] ),
	};

	static JsonObject TrackJson( IPresenceTrack t ) => new() {
		["revealed"] = new JsonArray( [.. t.Revealed.Select( x => (JsonNode)((IOption)x).Text )] ),
		["covered"] = new JsonArray( [.. t.Slots.Except( t.Revealed ).Select( x => (JsonNode)((IOption)x).Text )] ),
	};

	internal static JsonObject CardJson( PowerCard c ) => new() {
		["name"] = c.Title, ["cost"] = c.Cost, ["speed"] = c.Speed.ToString(),
		["elements"] = c.Elements.BuildElementString( true ),
		["range"] = c.RangeText, ["target"] = c.TargetFilter, ["instructions"] = c.Instructions,
	};

	static JsonNode SpaceJson( Space space ) {
		var tokens = new JsonObject();
		foreach( IToken t in space.OfType<IToken>().OrderBy( t => t.Text ) )
			tokens[t.Text] = space[t];
		return new JsonObject {
			["name"] = space.SpaceSpec.Label,
			["terrain"] = string.Join( "/", Enum.GetValues<Terrain>().Where( t => t != Terrain.None && space.SpaceSpec.Is( t ) ) ),
			["adjacent"] = new JsonArray( [.. space.Adjacent_Existing.Select( a => (JsonNode)a.SpaceSpec.Label )] ),
			["tokens"] = tokens,
		};
	}
}
