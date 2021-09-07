using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {
	public class TwistedFlowersMurmurUltimatums {
        [MajorCard("", 4, Speed.Slow, Element.Sun, Element.Moon, Element.Fire, Element.Air, Element.Water, Element.Earth, Element.Plant, Element.Animal)]
        [FromPresence(0)]
        static public Task ActAsync(TargetSpaceCtx ctx) {
            return Task.CompletedTask;
        }
    }
}
