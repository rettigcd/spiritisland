namespace SpiritIsland.Basegame;

public class Retreat : IFearOptions {

	public const string Name = "Retreat";
	string IFearOptions.Name => Name;


	[FearLevel( 1, "Each player may Push up to 2 Explorer from an Inland land." )]
	public Task Level1( FearCtx ctx ) {
		return Cmd.EachSpirit( Cmd.PushUpToNExplorers( 2 ).From( x => !x.IsCoastal && x.IsInPlay, "Inland" ) )
			.Execute( ctx.GameState );
	}

	[FearLevel( 2, "Each player may Push up to 3 Explorer / Town from an Inland land." )]
	public Task Level2( FearCtx ctx ) {
		return Cmd.EachSpirit( Cmd.PushUpToNInvaders( 3, Invader.Explorer, Invader.Town ).From( x => !x.IsCoastal && x.IsInPlay, "Inland" ) ) // !! assuming Explorers and Town are the same.
			.Execute( ctx.GameState );
	}

	[FearLevel( 3, "Each player may Push any number of Explorer / Town from one land." )]
	public Task Level3( FearCtx ctx ) {
		return Cmd.EachSpirit( Cmd.PushUpToNInvaders( int.MaxValue, Invader.Explorer, Invader.Town ).From( _ => true, "" ))
				.Execute( ctx.GameState );
	}

}