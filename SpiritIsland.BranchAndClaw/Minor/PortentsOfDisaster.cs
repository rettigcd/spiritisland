namespace SpiritIsland.BranchAndClaw;

public class PortentsOfDisaster {

	const string Name = "Portents of Disaster";

	[MinorCard( Name, 0, Element.Sun, Element.Moon, Element.Air ), Fast, FromSacredSite( 1, Filter.Invaders )]
	[Instructions( "2 Fear. The next time an Invader is destroyed in target land this turn, 1 Fear." ), Artist( Artists.NolanNasser )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {
		// 2 fear
		await ctx.AddFear(2);

		// The next time an invader is destroyed in target land this turn, 1 fear
		ctx.Space.Adjust( new Add1FearForFirstDestroyedInvader(ctx), 1 );

	}

	public class Add1FearForFirstDestroyedInvader( TargetSpaceCtx ctx ) : BaseModEntity, IEndWhenTimePasses, IHandleTokenRemoved, ISerializableSpaceEntity {
		bool _addFear = true;
		public async Task HandleTokenRemovedAsync( ITokenRemovedArgs args ) {
			if( _addFear
				&& args.Reason.IsDestroy()
				&& args.Removed.HasTag(TokenCategory.Invader)
			){ // !! create an override .IsInvader()
				await ctx.AddFear(1);
				_addFear = false;
			}
		}

		JsonArray ISerializableSpaceEntity.ToJson( ISerializationContext serCtx )
			=> new JsonArray( Tag, serCtx.IndexOf( ctx.Self ), ctx.SpaceSpec.Label, _addFear );

		const string Tag = "Add1FearForFirstDestroyedInvader";

		[ModuleInitializer]
		internal static void RegisterSerialization()
			=> SpaceEntitySerialization.Register( Tag, ( json, serCtx ) => {
				Spirit spirit = serCtx.SpiritAt( (int)json[1]! );
				SpaceSpec spec = serCtx.SpaceSpecByLabel( json[2]!.GetValue<string>() );
				return new Add1FearForFirstDestroyedInvader( serCtx.TargetSpace( spirit, spec ) ) { _addFear = json[3]!.GetValue<bool>() };
			} );
	}

}