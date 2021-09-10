using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class ElementalBoon {

		[MinorCard( "Elemental Boon", 1, Speed.Fast )]
		[TargetSpirit]
		static public async Task Act( TargetSpiritCtx ctx ) {
			var target = ctx.Target;

			// Target Spirit gains 3 _different_ Elements of their choice
			var selected = await target.SelectElements( 3, Element.Sun, Element.Moon, Element.Air, Element.Fire, Element.Water, Element.Earth, Element.Plant, Element.Animal );
			target.Elements.AddRange( selected );

			// if you target another spirit, you also gain the chosen elements
			if(target != ctx.Self)
				ctx.Self.Elements.AddRange( selected );
		}

	}

}
