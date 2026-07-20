namespace SpiritIsland.JaggedEarth;

public class EntrapTheForcesOfCorruption{ 
		
	[MinorCard("Entrap the Forces of Corruption",1,Element.Earth,Element.Plant,Element.Animal),Fast,FromPresence(1)]
	[Instructions( "Gather up to 1 Blight. Isolate target land. When Blight is added to target land, it doesn't cascade." ), Artist( Artists.ShawnDaley )]
	static public async Task ActAsync( TargetSpaceCtx ctx ){
		// Gather up to 1 Blight
		await ctx.GatherUpTo(1,Token.Blight);

		// Isolate target land.
		ctx.Isolate();

		// When blight is added to target land, it doesn't cascade.
		ctx.Space.Init(new StopCascade(),1);
	}

	public class StopCascade : BaseModEntity, IEndWhenTimePasses, IModifyAddingToken, ISerializableSpaceEntity {
		public Task ModifyAddingAsync( AddingTokenArgs args ) {
			if(args.Token == Token.Blight)
				BlightToken.ScopeConfig.ShouldCascade = false;
			return Task.CompletedTask;
		}

		JsonArray ISerializableSpaceEntity.ToJson( ISerializationContext ctx ) => new JsonArray( Tag );

		const string Tag = "StopCascade";

		[ModuleInitializer]
		internal static void RegisterSerialization()
			=> SpaceEntitySerialization.Register( Tag, ( json, ctx ) => new StopCascade() );
	}

}