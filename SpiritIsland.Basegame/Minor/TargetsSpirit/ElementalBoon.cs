using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class ElementalBoon {

		[MinorCard( "Elemental Boon", 1)]
		[Fast]
		[TargetSpirit]
		static public async Task Act( TargetSpiritCtx ctx ) {

			// Target Spirit gains 3 _different_ Elements of their choice
			Element[] elements = await Gain3DifferentElements( ctx.Other );

			// if you target another spirit, you also gain the chosen elements
			if(ctx.Other != ctx.Self)
				ctx.Self.Elements.AddRange( elements );
		}

		static async Task<Element[]> Gain3DifferentElements( Spirit spirit ) {
			var selected = await spirit.SelectElements( 3, Element.Sun, Element.Moon, Element.Air, Element.Fire, Element.Water, Element.Earth, Element.Plant, Element.Animal );
			spirit.Elements.AddRange( selected );
			return selected;
		}
	}

}
