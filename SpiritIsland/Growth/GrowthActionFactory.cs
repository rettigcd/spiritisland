﻿
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public abstract class GrowthActionFactory : IActionFactory {

		public virtual string Name => ToString().Split('.').Last();
		string IOption.Text => Name;

		public abstract Task ActivateAsync(SelfCtx ctx);

		public bool CouldActivateDuring( Phase speed, Spirit _ ) => speed == Phase.Growth;

	}

}
