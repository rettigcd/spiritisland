using SpiritIsland.NatureIncarnate;

namespace SpiritIsland.Tests.Spirits.ToweringRoots;

public class EntwineTheFatesOfAll_Tests : ToweringRoots_Base {
	public EntwineTheFatesOfAll_Tests():base() {}

	[Fact]
	public async Task IncarnaDefends2() {
		// Given: Presence and Incarna on A2
		SpaceSpec space = _board[2];
		space.ScopeSpace.Init( _spirit.Presence.Token, 1 );
		Given_InvarnaOn( space );

		await _spirit.When_ResolvingCard<EntwineTheFatesOfAll>( u => {
			u.NextDecision.HasPrompt( "Select space to defend 2/presence." ).HasOptions( "A2" ).Choose( "A2" );
		} ).ShouldComplete();

		space.ScopeSpace.Defend.Count.ShouldBe(2*2);
	}
	void Given_InvarnaOn( SpaceSpec space ) {
		space.ScopeSpace.Init( _presence.Incarna, 1 );
	}

}
