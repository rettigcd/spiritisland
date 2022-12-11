namespace SpiritIsland.Basegame;

public class Retreat : IFearCard {

	public const string Name = "Retreat";
	public string Text => Name;
	public int? Activation { get; set; }
	public bool Flipped { get; set; }

	[FearLevel( 1, "Each player may Push up to 2 Explorer from an Inland land." )]
	public Task Level1( GameCtx ctx ) {
		return Cmd.EachSpirit( Cmd.PushUpToNExplorers( 2 ).From( x => !x.IsCoastal && x.IsInPlay, "Inland" ) )
			.Execute( ctx );
	}

	[FearLevel( 2, "Each player may Push up to 3 Explorer / Town from an Inland land." )]
	public Task Level2( GameCtx ctx ) {
		return Cmd.EachSpirit( Cmd.PushUpToNInvaders( 3, Invader.Explorer, Invader.Town ).From( x => !x.IsCoastal && x.IsInPlay, "Inland" ) ) // !! assuming Explorers and Town are the same.
			.Execute( ctx );
	}

	[FearLevel( 3, "Each player may Push any number of Explorer / Town from one land." )]
	public Task Level3( GameCtx ctx ) {
		return Cmd.EachSpirit( Cmd.PushUpToNInvaders( int.MaxValue, Invader.Explorer, Invader.Town ).From( _ => true, "" ))
				.Execute( ctx );
	}

}