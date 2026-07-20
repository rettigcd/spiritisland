namespace SpiritIsland.Basegame;

public class MistsOfOblivion {

	const string Name = "Mists of Oblivion";

	[MajorCard( Name, 4, Element.Moon, Element.Air, Element.Water ),Slow,FromPresence(3)]
	[Instructions( "1 Fear per Town / City this Power destroys (to a maximum of 4). 1 Damage to each Invader. -If you have- 2 Moon, 3 Air, 2 Water: 3 Damage." ), Artist( Artists.NolanNasser )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		// 1 fear per town/city this power destroys (to a max of 4)
		ctx.Space.Adjust( new FearOnTownCityDestroy( ctx ), 1 );

		// 1 damage to each invader
		await ctx.DamageEachInvader(1);

		// if you have 2 moon 3 air 2 water
		if(await ctx .YouHave("2 moon,3 air,2 water"))
			// 3 damage
			await ctx.DamageInvaders(3);
	}

	public class FearOnTownCityDestroy : BaseModEntity, IEndWhenTimePasses, IHandleTokenRemoved, ISerializableSpaceEntity {

		public FearOnTownCityDestroy( TargetSpaceCtx ctx ) {
			_ctx = ctx;
			// only applies within this action - remove self once it ends
			ActionScope.Current.AtEndOfThisAction( _ => ctx.Space.Adjust( this, -1 ) );
		}

		readonly TargetSpaceCtx _ctx;
		int _mayDestroy = 4;

		public async Task HandleTokenRemovedAsync( ITokenRemovedArgs args ) {
			if(0 < _mayDestroy
				&& args.Reason == RemoveReason.Destroyed
				&& args.Removed.Class.IsOneOf( Human.Town_City )
			) {
				int fearToGrant = Math.Min( _mayDestroy, args.Count );
				await _ctx.AddFear( fearToGrant );
				_mayDestroy -= fearToGrant;
			}
		}

		#region Serialization

		[ModuleInitializer]
		internal static void RegisterSerialization() => SpaceEntitySerialization.Register( Tag, FromJson );

		const string Tag = "FearOnTownCityDestroy";

		JsonArray ISerializableSpaceEntity.ToJson( ISerializationContext ctx )
			=> new JsonArray( Tag, ctx.IndexOf( _ctx.Self ), _ctx.SpaceSpec.Label, _mayDestroy );

		static object FromJson( JsonArray json, ISerializationContext ctx ) {
			Spirit spirit = ctx.SpiritAt( (int)json[1]! );
			SpaceSpec spec = ctx.SpaceSpecByLabel( json[2]!.GetValue<string>() );
			var result = new FearOnTownCityDestroy( ctx.TargetSpace( spirit, spec ) );
			result._mayDestroy = (int)json[3]!;
			return result;
		}

		#endregion

	}

}