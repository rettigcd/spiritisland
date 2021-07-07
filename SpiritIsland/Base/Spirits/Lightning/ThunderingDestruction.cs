using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiritIsland.Base {

	// Innate:  Thundering Destruction => slow, 1 from sacred, any
	// 3 fire 2 air    destroy 1 town
	// 4 fire 3 air    you may instead destroy 1 city
	// 5 fire 4 air 1 water    also, destroy 1 town or city
	// 5 fire 5 air 2 water    also, destroy 1 town or city

	[Core.InnatePower("Thundering Destruction",Speed.Fast)]
	[Core.InnateOption(Element.Fire,Element.Animal,Element.Moon)]
	class ThunderingDestruction {
	}

}
