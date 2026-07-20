namespace SpiritIsland.BranchAndClaw;

public class ConfoundingMists {

	public const string Name = "Confounding Mists";

	[MinorCard( Name, 1, Element.Air, Element.Water ),Fast,FromPresence( 1 )]
	[Instructions( "Defend 4. -or- Each Invader added to target land this turn may be immediately Pushed to any adjacent land." ), Artist( Artists.LoicBelliau )]
	static public Task ActAsync( TargetSpaceCtx ctx ) {
		return ctx.SelectActionOption(
			new SpaceAction("Defend 4", ctx => ctx.Defend(4) ),
			new SpaceAction(
				"Invaders added to target are immediately pushed",
				ctx => ctx.Space.Adjust( new MistPusher( ctx.Self ), 1 )
			)
		);
	}

	public class MistPusher( Spirit _spirit ) : BaseModEntity, IHandleTokenAdded, IEndWhenTimePasses, ISerializableSpaceEntity {
		public async Task HandleTokenAddedAsync( Space to, ITokenAddedArgs args ) {
			// each invader added to target land this turn may be immediatley pushed to any adjacent land
			if(	args.Added.Class.IsOneOf(Human.Invader)
				&& args.Reason.IsOneOf( AddReason.Added, AddReason.MovedTo, AddReason.Explore, AddReason.Build )
			)
				await to.SourceSelector
					.UseQuota(new Quota().AddGroup(1,args.Added.Class))
					.PushN( _spirit );
		}

		JsonArray ISerializableSpaceEntity.ToJson( ISerializationContext ctx ) => new JsonArray( Tag, ctx.IndexOf( _spirit ) );

		const string Tag = "MistPusher";

		[ModuleInitializer]
		internal static void RegisterSerialization()
			=> SpaceEntitySerialization.Register( Tag, ( json, ctx ) => new MistPusher( ctx.SpiritAt( (int)json[1]! ) ) );
	}

}