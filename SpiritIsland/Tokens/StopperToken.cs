namespace SpiritIsland;

/// <summary> Stops either 1 or ALL builds. </summary>
public interface IBuildStopper : Token {
	bool Stops( TokenClass tokenClass );
	Task StopBuild( GameState gameState, SpaceState space );
}

/// <summary> Stops either 1 or ALL builds. </summary>
public class BuildStopper : UniqueToken, IBuildStopper {
	readonly TokenClass[] stoppedClasses;

	static public BuildStopper Default( string label ) => new BuildStopper( label, EDuration.OneStopThisTurn,  Invader.Town, Invader.City );
	
	static public BuildStopper StopAll( string label ) => new BuildStopper( label, EDuration.AllStopsThisTurn, Invader.Town, Invader.City );

	public BuildStopper( string label, EDuration duration, params TokenClass[] stoppedTokenClasses ) : base( label ) {
		Duration = duration;
		this.stoppedClasses = stoppedTokenClasses;
	}
	public EDuration Duration { get; }

	public bool Stops( TokenClass tokenClass ) => stoppedClasses.Contains( tokenClass );
	public virtual Task StopBuild( GameState _, SpaceState space ) {
		if(Duration == EDuration.OneStopThisTurn )
			space.Adjust( this, -1 ); // remove this token
		return Task.CompletedTask;
	}

	public enum EDuration { 
		OneStopThisTurn, // first, default
		AllStopsThisTurn
	};

}

