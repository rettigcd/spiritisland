using System.Threading.Tasks;


namespace SpiritIsland.Basegame {

	[InnatePower( LeadTheFuriousAssult.Name,Speed.Slow)]
	[FromPresence(0)]
	public class LeadTheFuriousAssult {
		public const string Name = "Lead the Furious Assult";

		[InnateOption( "2 sun, 1 fire" )]
		static public Task Option1Async(TargetSpaceCtx ctx ) {
			// Destroy 1 town for every 2 dahan
			return ctx.Invaders
				.Destroy(ctx.GameState.DahanGetCount(ctx.Space)/2, Invader.Town );
		}

		[InnateOption( "4 sun, 3 fire" )]
		static public async Task Option2Async( TargetSpaceCtx ctx ) {
			// Destroy 1 city for every 3 dahan
			await ctx.Invaders
				.Destroy( ctx.GameState.DahanGetCount( ctx.Space ) / 3, Invader.City );

			await Option1Async(ctx);
		}
	}

	class FastIf4Air<T> : InnatePower_TargetSpace {
		public FastIf4Air() : base( typeof( T ) ) { }

		public override void UpdateFromSpiritState( CountDictionary<Element> elements ) {
			base.UpdateFromSpiritState( elements );
			OverrideSpeed = elements.Contains("4 air") ? new SpeedOverride( Speed.FastOrSlow, LeadTheFuriousAssult.Name )  : null;
		}

	}

	// This is hard - need to allow attribute to dynamically update innate speed

	//class InnateConditinalFastAttribute : InnatePowerAttribute {
	//	string fastTriggerElements;
	//	public InnateConditinalFastAttribute( string name, Speed speed, string triggerElements )
	//		: base( name, speed ) {
	//		this.fastTriggerElements = triggerElements;
	//	}

	//	public override void UpdateFromSpiritState( CountDictionary<Element> elements, InnatePower card ) {
	//		// if you have 3 air, this power may be fast
	//		card.OverrideSpeed = elements.Contains( fastTriggerElements ) ? Speed.FastOrSlow : null;
	//	}

	//}

}
