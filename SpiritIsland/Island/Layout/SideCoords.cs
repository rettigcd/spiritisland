namespace SpiritIsland;

public record SideCoords( BoardCoord From, BoardCoord To ) {
	public BoardCoord Angle => To - From;
	public int RotationStep => Array.IndexOf( BoardCoord.Angles, Angle );
}
