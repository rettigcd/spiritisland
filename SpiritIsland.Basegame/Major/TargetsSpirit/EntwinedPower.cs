namespace SpiritIsland.Basegame;

public class EntwinedPower {


	[MajorCard( "Entwined Power", 2, Element.Moon, Element.Water, Element.Plant ),Fast,AnotherSpirit]
	[Instructions( "You and target Spirit may use each other's Presence to target Powers. Target Spirit gains a Power Card. You gain one of the power Cards they did not keep. -If you have- 2 Water, 4 Plant: You and target Spirit each gain 3 Energy and may gift the other 1 Power from hand." ), Artist( Artists.JoshuaWright )]
	static public async Task ActAsync( TargetSpiritCtx ctx ) {

		// You and other spirit share presence for targeting
		if( ctx.Self != ctx.Other) {
			// Save off to restore later
			var selfOrig = ctx.Self.TargetingSourceStrategy;
			var otherOrig = ctx.Other.TargetingSourceStrategy;
			GameState.Current.AddTimePassesAction( new RestoreTargetingSourceStrategy( ctx.Self, selfOrig, ctx.Other, otherOrig ) );
			// set new
			_ = new EntwinedPresenceSource( ctx.Self, ctx.Other ); // auto-binds to spirits
		}

		// Target spirit gains a power Card.
		DrawCardResult result = await ctx.Other.Draw.Card();
		// You gain one of the power Cards they did not keep.
		ctx.Self.Hand.Add( 
			await DrawFromDeck.PickOutCard( ctx.Self, [.. result.Rejected] )
		);

		// if you have 2 water, 4 plant, 
		if(await ctx.YouHave("2 water,4 plant")) {
			// you and target spirit each gain 3 energy
			ctx.Self.Energy += 3;
			ctx.Other.Energy += 3;
			// may gift the other 1 power from hand.
			await GiftCardToSpirit( ctx.Self, ctx.Other );
			await GiftCardToSpirit( ctx.Other, ctx.Self );
		}
	}

	static async Task GiftCardToSpirit( Spirit src, Spirit dst ) {
		var myGift = await src.SelectPowerCard( "Select gift for " + dst.SpiritName, 1, src.Hand.ToArray(), CardUse.Gift, Present.Done );
		if(myGift != null) {
			dst.Hand.Add( myGift );
			src.Hand.Remove( myGift );
		}
	}


	internal class EntwinedPresenceSource : ITargetingSourceStrategy {

		readonly Dictionary<Spirit, ITargetingSourceStrategy> _olds;

		public EntwinedPresenceSource(params Spirit[] spirits) {
			_olds = spirits.ToDictionary(s => s, s => s.TargetingSourceStrategy);

			foreach( Spirit spirit in spirits )
				spirit.TargetingSourceStrategy = this;
		}

		// Used by FromJson to rebuild the exact captured _olds snapshot rather than recomputing "each
		// spirit's current strategy" (which, mid-restore, wouldn't be the originally-saved value) and
		// without re-assigning TargetingSourceStrategy on each spirit - that's Spirit.RestoreFromJson's
		// own job for whichever spirit's restore is currently running.
		EntwinedPresenceSource( Dictionary<Spirit, ITargetingSourceStrategy> olds ) => _olds = olds;

		public IEnumerable<Space> EvaluateFrom(IKnowSpiritLocations presence, TargetFrom from) {
			return _olds
				.SelectMany(p => p.Value.EvaluateFrom(p.Key.Presence, from))
				.Distinct();
		}

		// [ Tag, [ [spiritIndex, oldStrategyJson], ... ] ] - each spirit's pre-entwined strategy resolves
		// recursively through this same registry (in practice always "Default", but not assumed).
		// Note: if both entwined spirits are independently restored, each ends up with its own
		// EntwinedPresenceSource instance (not the single shared one live gameplay uses) - harmless here
		// since nothing compares TargetingSourceStrategy by reference, unlike section 2's InnatePower/
		// SpiritPresenceToken identity fixes.
		JsonArray ITargetingSourceStrategy.ToJson( ISerializationContext ctx ) => new JsonArray(
			Tag, new JsonArray( _olds.Select( p => (JsonNode)new JsonArray( ctx.IndexOf( p.Key ), p.Value.ToJson( ctx ) ) ).ToArray() )
		);

		const string Tag = "EntwinedPresenceSource";

		[ModuleInitializer]
		internal static void RegisterSerialization()
			=> TargetingSourceStrategyRegistry.Register( Tag, ( json, ctx ) => new EntwinedPresenceSource(
				( (JsonArray)json[1]! ).Select( n => (JsonArray)n! )
					.ToDictionary( pair => ctx.SpiritAt( pair[0]!.GetValue<int>() ), pair => TargetingSourceStrategyRegistry.Deserialize( (JsonArray)pair[1]!, ctx ) )
			) );

	}

	/// <summary>
	/// Rolls back the TargetingSourceStrategy override EntwinedPresenceSource set, restoring each
	/// spirit's pre-entwined strategy. Captures 2 Spirits (index-resolvable) plus 2
	/// ITargetingSourceStrategy snapshots (resolved recursively through TargetingSourceStrategyRegistry,
	/// same as EntwinedPresenceSource's own _olds above).
	/// </summary>
	internal class RestoreTargetingSourceStrategy( Spirit self, ITargetingSourceStrategy selfOrig, Spirit other, ITargetingSourceStrategy otherOrig )
		: IRunWhenTimePasses, ISerializableTimePassesAction {

		bool IRunWhenTimePasses.RemoveAfterRun => true;
		TimePassesOrder IRunWhenTimePasses.Order => TimePassesOrder.Normal;
		Task IRunWhenTimePasses.TimePasses( GameState gameState ) {
			self.TargetingSourceStrategy = selfOrig;
			other.TargetingSourceStrategy = otherOrig;
			return Task.CompletedTask;
		}

		const string Tag = "EntwinedPower.RestoreTargetingSourceStrategy";

		JsonArray ISerializableTimePassesAction.ToJson( ISerializationContext ctx ) => new JsonArray(
			Tag, ctx.IndexOf( self ), selfOrig.ToJson( ctx ), ctx.IndexOf( other ), otherOrig.ToJson( ctx )
		);

		[ModuleInitializer]
		internal static void RegisterSerialization()
			=> TimePassesActionRegistry.Register( Tag, ( json, ctx ) => new RestoreTargetingSourceStrategy(
				ctx.SpiritAt( json[1]!.GetValue<int>() ),
				TargetingSourceStrategyRegistry.Deserialize( (JsonArray)json[2]!, ctx ),
				ctx.SpiritAt( json[3]!.GetValue<int>() ),
				TargetingSourceStrategyRegistry.Deserialize( (JsonArray)json[4]!, ctx )
			) );

	}

}
