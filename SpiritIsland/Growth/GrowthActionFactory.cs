
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public abstract class GrowthActionFactory : IActionFactory {

		public virtual string Name => ToString().Split('.').Last();
		string IOption.Text => Name;

		public abstract Task ActivateAsync(SpiritGameStateCtx ctx);

		public bool IsActiveDuring( Speed speed, CountDictionary<Element> _ ) => speed == Speed.Growth;

	}

}
