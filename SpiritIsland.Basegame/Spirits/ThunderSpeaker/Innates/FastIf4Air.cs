
namespace SpiritIsland.Basegame {

	public class FastIf4Air<T> : InnatePower_TargetSpace {

		public FastIf4Air() : base( typeof( T ) ) { }

		public override void UpdateFromSpiritState( CountDictionary<Element> elements ) {
			base.UpdateFromSpiritState( elements );
			OverrideSpeed = elements.Contains("4 air") ? new SpeedOverride( Speed.FastOrSlow, LeadTheFuriousAssult.Name )  : null;
		}

	}

}
