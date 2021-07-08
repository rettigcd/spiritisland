using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	// Innate:  Thundering Destruction => slow, 1 from sacred, any
	// 3 fire 2 air    destroy 1 town
	// 4 fire 3 air    you may instead destroy 1 city
	// 5 fire 4 air 1 water    also, destroy 1 town or city
	// 5 fire 5 air 2 water    also, destroy 1 town or city

	[Core.InnatePower( ThunderingDestruction.Name, Speed.Fast )]
	[Core.InnateOption( Element.Fire, Element.Fire, Element.Fire, Element.Air, Element.Air )]
	class ThunderingDestruction : TargetSpaceAction {
		public const string Name = "Thundering Destruction";

		public Spirit lightning;
		public ThunderingDestruction(Spirit lightning,GameState gs):base(lightning,gs,1,From.SacredSite){
			this.lightning = lightning;


		}

		protected override void SelectSpace( Space space ) {
			var grp = gameState.InvadersOn(space);
			var decision = SelectDecision(grp);
			if(decision==null) return;

			engine.decisions.Push(decision);
			engine.actions.Add(new SimpleAction(()=>{ gameState.UpdateFromGroup(grp); } ));
		}

		IDecision SelectDecision(InvaderGroup grp){
			int fire = lightning.Elements(Element.Fire);
			int air = lightning.Elements(Element.Air);
			int water = lightning.Elements(Element.Water);

			return fire >=5 && air>=5 && water>=2	? DestroyTownsOrCities(grp,3)
				: fire >=5 && air>=4 && water>=1	? DestroyTownsOrCities(grp,2)
                : fire >=4 && air>=3				? DestroyTownsOrCities(grp,1)
                : fire >=3 && air>=2				? DestroyTowns(grp,1)
                : null;
		}

		IDecision DestroyTownsOrCities(InvaderGroup grp, int count)
			=> new SelectInvadersToDestroy(engine,grp,count,"C@3","C@2","C@1","T@2","T@1");

		IDecision DestroyTowns(InvaderGroup grp, int count)
			=> new SelectInvadersToDestroy(engine,grp,count,"T@2","T@1");

	}

}
