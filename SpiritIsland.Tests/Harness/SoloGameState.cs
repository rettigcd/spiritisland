#nullable enable
namespace SpiritIsland.Tests;

/// <summary>
/// Helper Test class to make it easier to spin up Solo games 
/// and access the Spirit and Board
/// </summary>
public class SoloGameState(Spirit spirit, Board board) 
	: GameState([spirit], [board], 0)
{
	#region constructors

	/// <summary> When you don't care about the Spirit nor the Board </summary>
	public SoloGameState() : this( DefaultSpirit, DefaultBoard ) { }
	/// <summary> When you care about the Spirit only. </summary>
	public SoloGameState(Spirit spirit) : this(spirit, DefaultBoard) { }
	/// <summary> When you care about the Board only. </summary>
	public SoloGameState(Board board) : this(DefaultSpirit, board) { }

	#endregion constructors

	public Board Board { get; } = board;
	public Spirit Spirit { get; } = spirit;

	static Spirit DefaultSpirit => new TestSpirit(PowerCard.For(typeof(IndomitableClaim)));
	static Board DefaultBoard => Boards.A;
}