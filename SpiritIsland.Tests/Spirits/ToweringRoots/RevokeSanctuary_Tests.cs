using SpiritIsland.NatureIncarnate;

namespace SpiritIsland.Tests.Spirits.ToweringRoots;

public class RevokeSanctuary_Tests {

	public RevokeSanctuary_Tests() {
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

	void Given_IncarnaOn( Space space ) {
		space.Tokens.Init( _presence.Incarna, 1 );
	}

}

public class ToweringRoots_Invarna_Tests {

	public ToweringRoots_Invarna_Tests() {
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
	public async Task IncarnaProtectsDahanDuringRavage() {
		var tokens = _board[8].Tokens;
		// Given Dahan and town
		tokens.Dahan.AddDefault(1);
		tokens.InitDefault(Human.Town,1);
		// Given Incarna, Vitality
		Given_IncarnaOn( tokens.Space );
		tokens.Init(Token.Vitality,1);

		// When ravage
		await tokens.Ravage();

		// Then: resulting tokens
		tokens.Summary.ShouldBe("1D@2,1T@2,1TRotJ-");

	}

	void Given_IncarnaOn( Space space ) {
		space.Tokens.Init( _presence.Incarna, 1 );
	}

}
