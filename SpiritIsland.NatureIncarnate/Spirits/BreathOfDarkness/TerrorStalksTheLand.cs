namespace SpiritIsland.NatureIncarnate;

public class TerrorStalksTheLand(Spirit spirit) : BaseModEntity, IAdjustDamageToInvaders_FromSpiritPowers, IModifyRemovingToken, ISerializableSpaceEntity {

	public const string Name = "Terror Stalks the Land";
	const string Description = "You may Abduct 1 Explorer / Town at empowered Incarna each Fast Phase. To Abduct a piece, Move it to the Endless Dark."
		+ " When pieces Escape, Move them to a non-Ocean land with your Presence/Incarna. If they have no legal land to move to, you lose."
		+ " When your Powers would directly damage or directly destroy the only Invader in a land, instead Abduct it.";
	static public SpecialRule Rule => new SpecialRule(Name, Description);


	Task IAdjustDamageToInvaders_FromSpiritPowers.ModifyDamage(DamageFromSpiritPowers args) {
		if( spirit.ActionIsMyPower ) {
			var invaders = args.Space.HumanOfTag(TokenCategory.Invader);
			if( invaders.Length == 1 ) {
				args.Damage = 0;
				return invaders[0].On(args.Space).MoveTo(EndlessDark.Space.ScopeSpace);
			}
		}
		return Task.CompletedTask;
	}

	Task IModifyRemovingToken.ModifyRemovingAsync(RemovingTokenArgs args) {
		if( args.Count == 1 && args.Reason.IsDestroy() && spirit.ActionIsMyPower && args.From is Space space && args.Token is HumanToken ht ) {
			var invaders = space.HumanOfTag(TokenCategory.Invader);
			if( invaders.Length == 1 ) {
				args.Count = 0;
				return invaders[0].On(space).MoveTo(EndlessDark.Space.ScopeSpace);
			}
		}

		return Task.CompletedTask;
	}

	#region Serialization

	[ModuleInitializer]
	internal static void RegisterSerialization() => SpaceEntitySerialization.Register( Tag, FromJson );

	const string Tag = "TerrorStalksTheLand";

	JsonArray ISerializableSpaceEntity.ToJson( ISerializationContext ctx ) => new JsonArray( Tag, ctx.IndexOf( spirit ) );

	static object FromJson( JsonArray json, ISerializationContext ctx )
		=> new TerrorStalksTheLand( ctx.SpiritAt( (int)json[1]! ) );

	#endregion

}
