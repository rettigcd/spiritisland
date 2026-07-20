namespace SpiritIsland;

public class TokenVariety(TokenClassToken original, string badge) : IToken, ISerializableSpaceEntity {
	#region IToken
	Img IToken.Img => _asToken.Img;
	ITokenClass IToken.Class => original;
	string IToken.Badge => badge;
	string IToken.SpaceAbreviation => $"{_asToken.SpaceAbreviation}-{badge}";
	bool IToken.HasTag(ITag tag) => _asToken.HasTag(tag);
	#endregion IToken
	string IOption.Text => $"{_asClass.Label}-{badge}";

	IToken _asToken => original;
	ITokenClass _asClass => original;

	JsonArray ISerializableSpaceEntity.ToJson( ISerializationContext ctx ) => new JsonArray( Tag, original.Label, badge );

	const string Tag = "TokenVariety";

	[ModuleInitializer]
	internal static void RegisterSerialization()
		=> SpaceEntitySerialization.Register( Tag, ( json, ctx )
			=> new TokenVariety( (TokenClassToken)ctx.TokenClassByLabel( json[1]!.GetValue<string>() ), json[2]!.GetValue<string>() ) );
}