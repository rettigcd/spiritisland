namespace SpiritIsland.Web.View;

/// <summary>
/// Projects the live GameState into what a player legitimately sees. Modelled on the MCP server's PlayerObservation, whose
/// rules for what is hidden apply here: invader-deck order and contents, power-card deck order, face-down fear cards,
/// unused invader cards and the unflipped blight card are never read, only counted.
/// Plain engine reads and plain records: nothing here knows about the web.
/// </summary>
public static class GameViewBuilder {

	/// <param name="viewer">The spirit that belongs to the person looking.</param>
	public static GameView Build( GameState gs, Spirit viewer ) {
		ActionScope.Initialize( gs.RootScope ); // engine state is read through the AsyncLocal scope
		return new GameView(
			gs.RoundNumber, gs.Phase.ToString(),
			FearOf( gs ), BlightOf( gs ), InvadersOf( gs ),
			viewer.SpiritName, [.. gs.Spirits.Select( SpiritOf )],
			[.. gs.Spaces_Unfiltered.Select( SpaceOf )] );
	}

	static FearView FearOf( GameState gs ) => new(
		gs.Fear.TerrorLevel, gs.Fear.EarnedFear, gs.Fear.PoolMax,
		gs.Fear.Deck.Count, // count only; identities are hidden
		// Earned cards lie face down until the next Invader Phase flips them, so only how many are waiting is public.
		gs.Fear.ActivatedCards.Count );

	static BlightView BlightOf( GameState gs ) {
		BlightCard card = gs.BlightCard;
		// The card is set up healthy side up without looking at the back, so its name and Blighted Island effect stay
		// hidden until it flips. Only the blight count (printed on the front) is public before then.
		return new BlightView(
			card.CardFlipped,
			card.CardFlipped ? card.Name : null,
			card.CardFlipped ? card.Description : null,
			(int)card.Memento, // the only public accessor of the count
			gs.BlightCards.Count );
	}

	static InvadersView InvadersOf( GameState gs ) {
		InvaderDeck deck = gs.InvaderDeck;
		return new InvadersView(
			Texts( deck.Explore.Cards ), Texts( deck.Build.Cards ), Texts( deck.Ravage.Cards ), Texts( deck.Discards ),
			deck.UnrevealedCards.Count ); // order and contents are hidden
		static string[] Texts( IEnumerable<InvaderCard> cards ) => [.. cards.Select( c => ((IOption)c).Text )];
	}

	static SpiritView SpiritOf( Spirit s ) => new(
		s.SpiritName, s.SpiritName, s.Energy, s.Elements.Summary(),
		new PresenceView(
			RevealedOf( s.Presence.Energy ), RevealedOf( s.Presence.CardPlays ),
			s.Presence.EnergyPerTurn, s.Presence.CardPlayPerTurn, s.Presence.Destroyed.Count,
			[.. s.Presence.SacredSites.Select( space => space.SpaceSpec.Label )] ),
		[.. s.GrowthTrack.Groups.Where( g => g.Used ).Select( g => g.ToString() )],
		[.. s.Hand.Select( CardOf )], [.. s.InPlay.Select( CardOf )], [.. s.DiscardPile.Select( CardOf )],
		[.. s.InnatePowers.Select( p => new InnateView( p.Title,
			[.. p.DrawableOptions.Select( tier => s.Elements.CouldHave( tier.Elements ).ToString() )] ) )] );

	static TrackView RevealedOf( IPresenceTrack track ) {
		List<Track> slots = [.. track.Slots];
		return new TrackView( [.. track.Revealed.Select( slot => slots.IndexOf( slot ) )] );
	}

	static CardView CardOf( PowerCard c )
		=> new( c.Title, c.Title, c.Cost, c.Speed.ToString(), c.Elements.BuildElementString( true ) );

	/// <summary> Token positions come from the same layout the other UIs use, so they are always inside their space. </summary>
	static SpaceView SpaceOf( Space space ) {
		ManageInternalPoints points = new ManageInternalPoints( space.SpaceSpec.Layout ).Init( space );
		return new SpaceView( space.SpaceSpec.Label, [.. space.OfType<IToken>().OrderBy( t => t.Text ).Select( t => TokenOf( space, points, t ) )] );
	}

	static TokenView TokenOf( Space space, ManageInternalPoints points, IToken token ) {
		XY at = points.GetPointFor( token );
		return new TokenView(
			token.Text,
			token is SpiritPresenceToken presence ? "Presence:" + presence.Self.SpiritName : token.Img.ToString(),
			token.Text,
			space[token],
			token.Badge,
			token is HumanToken human ? human.StrifeCount : 0,
			new SpaceToken( space, token ).IsSacredSite,
			at.X, at.Y );
	}
}
