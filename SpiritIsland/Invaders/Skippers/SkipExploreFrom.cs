namespace SpiritIsland;

/// <summary> Stops space from being a source of Explorers</summary>
sealed public class SkipExploreFrom : ActionModBaseToken, ISkipExploreFrom {

	public SkipExploreFrom( string label ) : base( label ) { }

}
