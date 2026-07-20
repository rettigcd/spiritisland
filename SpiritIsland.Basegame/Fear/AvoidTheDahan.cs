namespace SpiritIsland.Basegame;

public class AvoidTheDahan : FearCardBase, IFearCard {

	public const string Name = "Avoid the Dahan";
	public string Text => Name;

	[FearLevel("Invaders do not Explore into lands with at least 2 Dahan." )]
	public override Task Level1( GameState gs )
		=> StopExploreInLandsWithAtLeast2Dahan
			.In().EachActiveLand()
			.ActAsync( gs );

	static SpaceAction StopExploreInLandsWithAtLeast2Dahan => new SpaceAction( "Stop Explore if 2 dahan",
		ctx => {
			var token = new StopExploreIn2DahanLands();
			ctx.Space.Adjust( token, 1 );
		}
	);

	public class StopExploreIn2DahanLands() : SkipExploreTo_Custom(true), ISerializableSpaceEntity {
		protected override bool ShouldSkip(Space space) => 2 <= space.Dahan.CountAll;

		JsonArray ISerializableSpaceEntity.ToJson( ISerializationContext ctx ) => new JsonArray( Tag );

		const string Tag = "StopExploreIn2DahanLands";

		[ModuleInitializer]
		internal static void RegisterSerialization()
			=> SpaceEntitySerialization.Register( Tag, ( json, ctx ) => new StopExploreIn2DahanLands() );
	}

	[FearLevel("Invaders do not Build in lands where Dahan outnumber Town/City." )]
	public override Task Level2( GameState gs )
		=> StopBuildWhereDahanOutnumberTownsCities
			.In().EachActiveLand()
			.ActAsync( gs );

	static SpaceAction StopBuildWhereDahanOutnumberTownsCities => new SpaceAction(
		"Stop Build if dahan outnumber towns/cities.",
		ctx => ctx.Space.Adjust(new StopBuildWhereDahanOutnumber(), 1 )
	);

	public class StopBuildWhereDahanOutnumber() : SkipBuild_Custom(Name, true), ISerializableSpaceEntity {
		protected override bool ShouldSkip(Space space)
			=> space.SumAny(Human.Town_City) < space.Dahan.CountAll;

		JsonArray ISerializableSpaceEntity.ToJson( ISerializationContext ctx ) => new JsonArray( Tag );

		const string Tag = "StopBuildWhereDahanOutnumber";

		[ModuleInitializer]
		internal static void RegisterSerialization()
			=> SpaceEntitySerialization.Register( Tag, ( json, ctx ) => new StopBuildWhereDahanOutnumber() );
	}

	[FearLevel("Invaders do not Build in lands with Dahan." )]
	public override Task Level3( GameState gs )
		=> DoNotBuildInLandsWithDahan
			.In().EachActiveLand()
			.ActAsync( gs );

	static SpaceAction DoNotBuildInLandsWithDahan => new SpaceAction( "Stop Build in lands with Dahan",
		ctx => {
			var token = new StopBuildInDahanLands();
			ctx.Space.Adjust( token, 1 );
		}
	);

	public class StopBuildInDahanLands() : SkipBuild_Custom(Name, true), ISerializableSpaceEntity {
		protected override bool ShouldSkip(Space space) => space.Dahan.Any;

		JsonArray ISerializableSpaceEntity.ToJson( ISerializationContext ctx ) => new JsonArray( Tag );

		const string Tag = "StopBuildInDahanLands";

		[ModuleInitializer]
		internal static void RegisterSerialization()
			=> SpaceEntitySerialization.Register( Tag, ( json, ctx ) => new StopBuildInDahanLands() );
	}

	[ModuleInitializer]
	internal static void RegisterSerialization()
		=> FearCardRegistry.Register( nameof( AvoidTheDahan ), () => new AvoidTheDahan() );

}
