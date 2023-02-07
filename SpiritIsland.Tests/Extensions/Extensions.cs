using SpiritIsland.Log;

namespace SpiritIsland.Tests;

static internal class Extensions {

	public static async Task PlaceOn( this SpiritPresence presence, Space space, GameState gameState ) {
		await using UnitOfWork scope = gameState.StartAction ( ActionCategory.Default );
		await gameState.Tokens[space].BindScope().Add( presence.Token, 1 );
	}

	#region Generating Explorer Action on a space

	static public InvaderCard BuildInvaderCard( this Space space ) {
		var terrain = new[] { Terrain.Wetland, Terrain.Sand, Terrain.Jungle, Terrain.Mountain }.First( space.Is );
		return terrain != Terrain.Ocean ? InvaderCard.Stage1( terrain ) : throw new ArgumentException( "Can't invade oceans" );
	}

	// !!! should these use slots or Engines?
	static public Task DoARavage( this SpaceState space ) 
		=> new RavageSlot().ActivateCard(  space.Space.BuildInvaderCard(), space.AccessGameState() );
	static public Task DoARavage( this Space space, GameState gs ) 
		=> new RavageSlot().ActivateCard( space.BuildInvaderCard(), gs );

	static public Task DoABuild( this Space space, GameState gameState )
		=> new BuildSlot().ActivateCard( space.BuildInvaderCard(), gameState );
	static public Task DoAnExplore( this Space space, GameState gs ) 
		=> new ExploreSlot().ActivateCard( space.BuildInvaderCard(), gs );


	#endregion

	/// <summary> Replaces all Invader Cards with null-cards that don't ravage/build/explore</summary>
	static public void DisableInvaderDeck( this GameState gs ) {
		var nullCard = InvaderCard.Stage1( Terrain.None );
		gs.InitTestInvaderDeck( new byte[12].Select( _ => nullCard ).ToArray() );
	}

	static public void IslandWontBlight( this GameState gameState ) => gameState.blightOnCard = 100;

	static public void Assert_Invaders( this GameState gameState, Space space, string expectedString ) {
		gameState.Tokens[space].InvaderSummary().ShouldBe( expectedString );
	}

	static public void Assert_DreamingInvaders( this GameState gameState, Space space, string expectedString ) {

		static int Order_CitiesTownsExplorers( HumanToken invader )
			=> -(invader.FullHealth * 10 + invader.RemainingHealth);
		var tokens = gameState.Tokens[space];
		string dreamerSummary = tokens.OfCategory(TokenCategory.Invader)
			.Cast<HumanToken>()
			.Where(x=>x.Class.Variant == TokenVariant.Dreaming)
			.OrderBy( Order_CitiesTownsExplorers )
			.Select( invader => tokens[invader] + invader.ToString() )
			.Join( "," );
		dreamerSummary.ShouldBe( expectedString );
	}

	static public void SetRevealedCount( this IPresenceTrack sut, int value ) {
		while(sut.Revealed.Count() < value)
			sut.Reveal( sut.RevealOptions.Single(), null );
	}

	static public string InvaderSummary( this SpaceState dict ) {

		// !!! Deprecate this.  Use .Invaders (to get just the invaders) then .Summary
		static int Order_CitiesTownsExplorers( HumanToken invader )
			=> -(invader.FullHealth * 10 + invader.RemainingHealth);
		return dict.InvaderTokens()
			.OrderBy( Order_CitiesTownsExplorers )
			.Select( invader => dict[invader] + invader.ToString() )
			.Join( "," );
	}

	static public void InitTestInvaderDeck(this GameState gameState, params InvaderCard[] cards ) {
		gameState.InvaderDeck = new InvaderDeck( cards.ToList(), null );// Don't try to inspect unused!
	}

	static public string Msg( this ILogEntry logEntry ) => logEntry.Msg( LogLevel.Info );

	static public TargetSpaceCtx TargetSpace( this SelfCtx ctx, string spaceLabel )
		=> ctx.Target( ctx.GameState.AllSpaces.Downgrade().First( s => s.Label == spaceLabel ) );

}
