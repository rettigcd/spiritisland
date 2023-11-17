namespace SpiritIsland;

/// <summary> A class-of token, not a token itself. </summary>
public interface ITokenClass : ITag {

	string Label { get; }

	bool HasTag(ITag tag);
}

public static class IEntityClassExtension {
	static public bool HasAny(this ITokenClass entityClass, params ITag[] tags) => tags.Any(entityClass.HasTag);
	static public bool HasAny( this IToken token, params ITag[] tags ) => tags.Any( token.HasTag );
}