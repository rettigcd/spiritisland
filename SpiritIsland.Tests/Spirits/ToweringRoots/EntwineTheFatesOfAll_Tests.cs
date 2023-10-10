using SpiritIsland.NatureIncarnate;

namespace SpiritIsland.Tests.Spirits.ToweringRoots;

public class EntwineTheFatesOfAll_Tests : ToweringRoots_Base {
	public EntwineTheFatesOfAll_Tests():base() {}

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
