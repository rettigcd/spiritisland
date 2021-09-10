using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class ScourTheLand {

		[MinorCardConditinalFast( "Scour the Land", 1, Speed.Slow, "3 air", Element.Air, Element.Earth )]
		[FromSacredSite( 2 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			await ctx.Invaders.Destroy(3,Invader.Town);
			await ctx.Invaders.Destroy(int.MaxValue,Invader.Explorer);

			ctx.AddBlight(1);

		}

	}

	class MinorCardConditinalFastAttribute : MinorCardAttribute {
		readonly string fastTriggerElements;
		public MinorCardConditinalFastAttribute( string name, int cost, Speed speed, string triggerElements, params Element[] elements )
			: base( name, cost, speed, elements ) {
			this.fastTriggerElements = triggerElements;
		}

		public override void UpdateFromSpiritState( CountDictionary<Element> elements, PowerCard card ) {
			// if you have 3 air, this power may be fast
			card.OverrideSpeed = elements.Contains(fastTriggerElements) ? new SpeedOverride( Speed.FastOrSlow, "scour the land" ) : null;
		}

	}

}
