namespace SpiritIsland.JaggedEarth;

public class SenseOfDread : FearCardBase, IFearCard {

	public const string Name = "Sense of Dread";
	public string Text => Name;

	[FearLevel(1, "On Each Board: Remove 1 Explorer from a land matching a Ravage card." )]
	public Task Level1( GameState ctx ) {
		return Cmd.RemoveExplorers( 1 ).From().OneLandPerBoard().Which( Is.RavageCardMatch )
			.ForEachBoard()
			.ActAsync( ctx );
	}

	[FearLevel(2, "On Each Board: Remove 1 Explorer/Town from a land matching a Ravage card." )]
	public Task Level2( GameState ctx ) { 
		return Cmd.RemoveExplorersOrTowns( 1 ).From().OneLandPerBoard().Which( Is.RavageCardMatch )
			.ForEachBoard()
			.ActAsync( ctx );
	}

	[FearLevel(3, "On Each Board: Remove 1 Invader from a land matching a Ravage card." )]
	public Task Level3( GameState ctx ) {
		return Cmd.RemoveInvaders( 1 ).From().OneLandPerBoard().Which( Is.RavageCardMatch )
			.ForEachBoard()
			.ActAsync( ctx );
	}

}