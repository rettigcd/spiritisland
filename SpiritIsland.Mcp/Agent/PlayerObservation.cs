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
		["fearPool"] = gs.Fear.EarnedFear,
		["fearPoolMax"] = gs.Fear.PoolMax,
		["facedownCardsRemaining"] = gs.Fear.Deck.Count, // count only; identities are hidden
		["revealedCards"] = new JsonArray( [.. gs.Fear.ActivatedCards.Select( c => (JsonNode)c.Text )] ),
	};

	static JsonObject BlightJson( GameState gs ) => new() {
		["card"] = gs.BlightCard.Name,
		["cardFlipped"] = gs.BlightCard.CardFlipped,
		["cardDescription"] = gs.BlightCard.Description,
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
			["unrevealedCount"] = deck.UnrevealedCards.Count, // order/contents hidden
		};
		static JsonArray Cards( IEnumerable<InvaderCard> cards ) => new( [.. cards.Select( c => (JsonNode)((IOption)c).Text )] );
	}

	static JsonObject SpiritJson( Spirit s ) => new() {
		["name"] = s.SpiritName,
		["energy"] = s.Energy,
		["elements"] = s.Elements.Summary(),
		["presence"] = new JsonObject {
			["energyTrack"] = TrackJson( s.Presence.Energy ),
			["cardPlayTrack"] = TrackJson( s.Presence.CardPlays ),
			["energyPerTurn"] = s.Presence.EnergyPerTurn,
			["cardPlaysPerTurn"] = s.Presence.CardPlayPerTurn,
			["destroyed"] = s.Presence.Destroyed.Count,
		},
		["growth"] = new JsonArray( [.. s.GrowthTrack.Groups.Select( g => (JsonNode)new JsonObject {
			["group"] = g.ToString(), ["used"] = g.Used, ["energyChange"] = g.GainEnergy,
		} )] ),
		["hand"] = new JsonArray( [.. s.Hand.Select( CardJson )] ),
		["inPlay"] = new JsonArray( [.. s.InPlay.Select( CardJson )] ),
		["innatePowers"] = new JsonArray( [.. s.InnatePowers.Select( p => (JsonNode)new JsonObject {
			["name"] = p.Title, ["speed"] = p.Speed.ToString(), ["target"] = p.TargetFilter, ["range"] = p.RangeText,
			["instructions"] = p.GeneralInstructions,
		} )] ),
	};

	static JsonObject TrackJson( IPresenceTrack t ) => new() {
		["revealed"] = new JsonArray( [.. t.Revealed.Select( x => (JsonNode)((IOption)x).Text )] ),
		["covered"] = new JsonArray( [.. t.Slots.Except( t.Revealed ).Select( x => (JsonNode)((IOption)x).Text )] ),
	};

	static JsonNode CardJson( PowerCard c ) => new JsonObject {
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
