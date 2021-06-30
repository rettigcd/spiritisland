﻿
using System.Linq;

namespace SpiritIsland.Core {

	public abstract class GrowthActionFactory : IActionFactory {

		public abstract IAction Bind(Spirit spirit, GameState gameState);

		public virtual string ShortDescription => ToString().Split('.').Last();

		public virtual string Name => this.ShortDescription;

		public void Resolved(Spirit spirit){
			spirit.UnresolvedActionFactories.Remove(this);
			if(spirit.UnresolvedActionFactories.Count==0)
				spirit.CollectEnergy();
		}

		public Speed Speed => Speed.Growth;

		public string Text => this.ShortDescription;
	}

}
