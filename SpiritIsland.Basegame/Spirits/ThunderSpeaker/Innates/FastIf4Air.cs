
namespace SpiritIsland.Basegame {

	public class FastIf4Air<T> : InnatePower_TargetSpace {

		public FastIf4Air() : base( typeof( T ) ) { }

		public override bool IsActiveDuring( Speed speed, CountDictionary<Element> elements ) {
			return base.IsActiveDuring( speed, elements )
				|| IsTriggered && elements.Contains("4 air") && speed == Speed.Fast;
		}

	}

}
