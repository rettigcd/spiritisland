namespace SpiritIsland.NatureIncarnate;

public class PiecesEscape : SpiritAction {

	static string CountToWord(int count) => count switch { int.MaxValue => "All", _ => count.ToString() };

	public PiecesEscape( int count = int.MaxValue ) 
		: base( $"{CountToWord(count)} pieces Escape" )
	{
		NumToEscape = count;
	}

	public override async Task ActAsync( SelfCtx ctx ) {
		await new TokenMover(ctx.Self,"Escape",EndlessDark.Space,ctx.Self.Presence.Lands.Tokens().ToArray())
			.AddGroup( NumToEscape, EndlessDark.Space.Tokens.ClassesPresent)
			.DoN();
	}
	public int NumToEscape { get; }
}
