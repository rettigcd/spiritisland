namespace SpiritIsland.NatureIncarnate;

public class PiecesEscape( int count = int.MaxValue ) : SpiritAction( $"{CountToWord(count)} pieces Escape" ) {

	static string CountToWord(int count) => count switch { int.MaxValue => "All", _ => count.ToString() };

	public override async Task ActAsync( Spirit self ) {
		await new TokenMover(self,"Escape",EndlessDark.Space,self.Presence.Lands.Tokens().ToArray())
			.AddGroup( NumToEscape, EndlessDark.Space.Tokens.ClassesPresent)
			.DoN();
	}
	public int NumToEscape { get; } = count;
}
