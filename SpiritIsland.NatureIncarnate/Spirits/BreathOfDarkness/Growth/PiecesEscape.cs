namespace SpiritIsland.NatureIncarnate;

public class PiecesEscape( int count = int.MaxValue ) 
	: SpiritAction( count switch { 1=>"1 piece Escapes", int.MaxValue=>"All pieces Escape", _ => $"{count} pieces Escape", } ) {

	public override async Task ActAsync( Spirit self ) {
		await new TokenMover(self,"Escape",EndlessDark.Space.ScopeSpace,self.Presence.Lands.ToArray())
			.AddGroup( NumToEscape, EndlessDark.Space.ScopeSpace.ClassesPresent)
			.DoN();
	}
	public int NumToEscape { get; } = count;
}
