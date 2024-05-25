namespace SpiritIsland.NatureIncarnate;

public class PiecesEscape( int count = int.MaxValue ) 
	: SpiritAction( count switch { 1=>"1 piece Escapes", int.MaxValue=>"All pieces Escape", _ => $"{count} pieces Escape", } ) {

	public override async Task ActAsync( Spirit self ) {
		Space space = EndlessDark.Space.ScopeSpace;
		var classes = space.ClassesPresent;
		if(0 < classes.Length)
			await new TokenMover(self,"Escape",space,self.Presence.Lands.ToArray())
				.AddGroup( NumToEscape, classes) // throws exception if no classes
				.DoN();
	}
	public int NumToEscape { get; } = count;
}
