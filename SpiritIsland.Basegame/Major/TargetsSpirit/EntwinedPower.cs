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
			GameState.Current.AddTimePassesAction(TimePassesAction.Once((_) => {
				ctx.Self.TargetingSourceStrategy = selfOrig;
				ctx.Other.TargetingSourceStrategy = otherOrig;
			}));
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


	class EntwinedPresenceSource : ITargetingSourceStrategy {

		readonly Dictionary<SpiritPresence, ITargetingSourceStrategy> _olds;

		public EntwinedPresenceSource(params Spirit[] spirits) {
			_olds = spirits.ToDictionary(s => s.Presence, s => s.TargetingSourceStrategy);

			foreach( Spirit spirit in spirits )
				spirit.TargetingSourceStrategy = this;
		}

		public IEnumerable<Space> EvaluateFrom(IKnowSpiritLocations presence, TargetFrom from) {
			return _olds
				.SelectMany(p => p.Value.EvaluateFrom(p.Key, from))
				.Distinct();
		}

	}

}
