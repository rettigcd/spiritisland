namespace SpiritIsland;

/// <summary> A class-of token, not a token itself. </summary>
/// <remarks>??? Is this any different from a ITag?  Could we just get rid of this interface?</remarks>
public interface ITokenClass : ITag {}

public static class IEntityClassExtension {
	static public bool HasAny( this IToken token, params ITag[] tags ) => tags.Any( token.HasTag );
}