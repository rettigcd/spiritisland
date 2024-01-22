namespace SpiritIsland;

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
public class FearLevelAttribute( string description ) : Attribute {

	public FearLevelAttribute(int _, string description ):this(description) { }

	public string Description { get; } = description;
}
