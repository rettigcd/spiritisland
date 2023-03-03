using SpiritIsland.Log;

namespace SpiritIsland.Tests;

static internal class Extensions {

	#region Generating Explorer Action on a space

	static public InvaderCard BuildInvaderCard( this Space space ) {
		var terrain = new[] { Terrain.Wetland, Terrain.Sand, Terrain.Jungle, Terrain.Mountain }.First( space.Is );
		return terrain != Terrain.Ocean ? InvaderCard.Stage1( terrain ) : throw new ArgumentException( "Can't invade oceans" );
	}

	static public Task When_Ravaging( this Space space ) => space.BuildInvaderCard().When_Ravaging();

	static public Task When_Building( this Space space ) => space.BuildInvaderCard().When_Building();

	static public Task When_Exploring( this Space space ) => space.BuildInvaderCard().When_Exploring();

	#endregion

	static public string InvaderSummary( this SpaceState dict ) {
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
		=> ctx.Target( ctx.GameState.Spaces_Unfiltered.Downgrade().First( s => s.Label == spaceLabel ) );

}
