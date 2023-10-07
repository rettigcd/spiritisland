using SpiritIsland.NatureIncarnate;

namespace SpiritIsland.Tests.Spirits.ToweringRoots;

public class EntwineTheFatesOfAll_Tests {
	public EntwineTheFatesOfAll_Tests() {
		_board = Board.BuildBoardA();
		_spirit = new ToweringRootsOfTheJungle();
		_presence = (ToweringRootsPresence)_spirit.Presence;
		_gs = new GameState( _spirit, _board );
	}
	readonly Board _board;
	readonly ToweringRootsOfTheJungle _spirit;
	readonly ToweringRootsPresence _presence;
	readonly GameState _gs;

	[Fact]
	public async Task IncarnaDefends2() {
		// Given: Presence and Incarna on A2
		Space space = _board[2];
		space.Tokens.Init( _spirit.Presence.Token, 1 );
		Given_InvarnaOn( space );

		await _spirit.When_ResolvingCard<EntwineTheFatesOfAll>( u => {
			u.NextDecision.HasPrompt( "Select space to defend 2/presence." ).HasOptions( "A2" ).Choose( "A2" );
		} ).ShouldComplete();

		space.Tokens.Defend.Count.ShouldBe(2*2);
	}
	void Given_InvarnaOn( Space space ) {
		space.Tokens.Init( _presence.Incarna, 1 );
	}

}
