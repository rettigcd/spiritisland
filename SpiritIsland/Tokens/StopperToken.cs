namespace SpiritIsland;

/// <summary> Stops either 1 or ALL builds. </summary>
public interface IBuildStopper : Token {
	bool Stops( TokenClass tokenClass );
	Task StopBuild( GameState gameState, Space space );
}

/// <summary> Stops either 1 or ALL builds. </summary>
public class BuildStopper : UniqueToken, IBuildStopper {
	readonly TokenClass[] stoppedClasses;

	static public BuildStopper Default( string label ) => new BuildStopper( label, Invader.Town, Invader.City );
	
	static public BuildStopper StopAll( string label ) => new BuildStopper( label, Invader.Town, Invader.City ) { 
		Duration = EDuration.AllStopsThisTurn
	};

	public BuildStopper( string label, params TokenClass[] stoppedTokenClasses ) : base( label ) {
		this.stoppedClasses = stoppedTokenClasses;
	}
	public bool Stops( TokenClass tokenClass ) => stoppedClasses.Contains( tokenClass );
	public virtual Task StopBuild( GameState gs, Space space ) {
		if(Duration == EDuration.OneStopThisTurn )
			gs.Tokens[space].Adjust( this, -1 ); // remove this token
		return Task.CompletedTask;
	}

	public EDuration Duration { get; set; } = EDuration.OneStopThisTurn;

	public enum EDuration { 
		OneStopThisTurn, // first, default
		AllStopsThisTurn
	};

}

