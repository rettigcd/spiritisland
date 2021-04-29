namespace SpiritIsland {

	public abstract class GrowthAction {
		protected Spirit spirit;
		protected GrowthAction(Spirit spirit){this.spirit=spirit;}
		public abstract void Apply();
	}


}
