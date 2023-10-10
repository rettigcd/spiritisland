using SpiritIsland.NatureIncarnate;

namespace SpiritIsland.Tests.Spirits.ToweringRoots;

public class RevokeSanctuary_Tests : ToweringRoots_Base {

	public RevokeSanctuary_Tests():base() {}

	[Fact]
	public async Task Trigger_Level1() {
		// Given: spirit has level-1 elements.
		_spirit.Elements[Element.Sun] = 1;
		_spirit.Elements[Element.Moon] = 1;
		_spirit.Elements[Element.Plant] = 2;

		// Given: Presence and Incarna on A8
		Space space = _board[8];
		Given_IncarnaOn(space);
		space.Tokens.Init(_presence.Token,1);
		
		//   And: 1 town
		space.Tokens.InitDefault(Human.Town,1);

		// When we resolve Innate
		await _spirit.When_ResolvingInnate<RevokeSanctuaryAndCastOut>( u => { 
			u.NextDecision.HasPrompt( "Revoke Sanctuary and Cast Out: Target Space" ).HasOptions("A8").Choose("A8");
			u.NextDecision.HasPrompt( "Remove up to (1)" ).HasOptions( "T@2,Done" ).Choose( "T@2" );
		} ).ShouldComplete();
		

	}

}
