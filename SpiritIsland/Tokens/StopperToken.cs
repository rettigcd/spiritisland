namespace SpiritIsland;

//internal class StopperToken : UniqueToken {
//	public StopperToken(string label, char initial) : base( label, initial, Img.None ) { }
//}

public interface IBuildStopper : Token {
	bool Stops( TokenClass tokenClass );
	Task StopBuild( GameState gameState, Space space );
}

public class BuildStopper : UniqueToken, IBuildStopper {
	readonly TokenClass[] stoppedClasses;
	public BuildStopper( string label, char initial, Img img, params TokenClass[] stoppedTokenClasses ) : base( label, initial, img ) {
		this.stoppedClasses = stoppedTokenClasses;
	}
	public bool Stops( TokenClass tokenClass ) => stoppedClasses.Contains( tokenClass );
	public Task StopBuild( GameState _, Space space ) => Task.CompletedTask;
}

