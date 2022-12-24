namespace SpiritIsland.Tests;

static internal class Extensions {

	public static Task PlaceOn( this SpiritPresence presence, Space space, GameState gameState )
		=> presence.PlaceOn( gameState.Tokens[space], gameState.StartAction( ActionCategory.Default ) );

	#region Generating Explorer Action on a space

	static public InvaderCard BuildInvaderCard( this Space space ) {
		var terrain = new[] { Terrain.Wetland, Terrain.Sand, Terrain.Jungle, Terrain.Mountain }.First( space.Is );
		return terrain != Terrain.Ocean ? InvaderCard.Stage1( terrain ) : throw new ArgumentException( "Can't invade oceans" );
	}

	static public Task DoARavage( this Space space, GameState gs ) => space.BuildInvaderCard().Ravage( gs );
	static public Task DoABuild( this Space space, GameState gs ) => space.BuildInvaderCard().Build( gs );
	static public Task DoAnExplore( this Space space, GameState gs ) => space.BuildInvaderCard().Explore( gs );


	#endregion

	/// <summary> Replaces all Invader Cards with null-cards that don't ravage/build/explore</summary>
	static public void DisableInvaderDeck( this GameState gs ) {
		var nullCard = InvaderCard.Stage1( Terrain.None );
		gs.InvaderDeck = InvaderDeck.BuildTestDeck( new byte[12].Select( _ => nullCard ).ToArray() );
	}

	static public void Assert_Invaders( this GameState gameState, Space space, string expectedString ) {
		gameState.Tokens[space].InvaderSummary().ShouldBe( expectedString );
	}

	static public void Assert_DreamingInvaders( this GameState gameState, Space space, string expectedString ) {

		static int Order_CitiesTownsExplorers( HealthToken invader )
			=> -(invader.FullHealth * 10 + invader.RemainingHealth);
		var tokens = gameState.Tokens[space];
		string dreamerSummary = tokens.OfCategory(TokenCategory.Invader)
			.Cast<HealthToken>()
			.Where(x=>x.Class.Variant == TokenVariant.Dreaming)
			.OrderBy( Order_CitiesTownsExplorers )
			.Select( invader => tokens.counts[invader] + invader.ToString() )
			.Join( "," );
		dreamerSummary.ShouldBe( expectedString );
	}

	static public void SetRevealedCount( this IPresenceTrack sut, int value ) {
		while(sut.Revealed.Count() < value)
			sut.Reveal( sut.RevealOptions.Single(), null );
	}

	static public string InvaderSummary( this SpaceState dict ) {

		// !!! Deprecate this.  Use .Invaders (to get just the invaders) then .Summary
		static int Order_CitiesTownsExplorers( HealthToken invader )
			=> -(invader.FullHealth * 10 + invader.RemainingHealth);
		return dict.InvaderTokens()
			.OrderBy( Order_CitiesTownsExplorers )
			.Select( invader => dict.counts[invader] + invader.ToString() )
			.Join( "," );
	}

}
