namespace SpiritIsland {

	public abstract class Spirit{

		/// <summary>
		/// Allow spirit to create custom PlayerState, to track any special spirit-stuff
		/// </summary>
		public virtual PlayerState CreateInitialPlayerState() => new PlayerState(this);

		public virtual void Grow(PlayerState ps, int option )
			=> this.GetGrowthOptions()[option].Apply( ps );

		public abstract GrowthOption[] GetGrowthOptions();

	}

}