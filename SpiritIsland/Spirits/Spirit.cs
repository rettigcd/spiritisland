namespace SpiritIsland {

	public abstract class Spirit : PlayerState {

		public Spirit():base(null){
			base.Spirit = this;
		}

		/// <summary>
		/// Allow spirit to create custom PlayerState, to track any special spirit-stuff
		/// </summary>
		public virtual PlayerState CreateInitialPlayerState() => this;

		public virtual void Grow(PlayerState ps, int optionIndex, IResolver[] resolvers ){
			GrowthOption option = this.GetGrowthOptions()[optionIndex];
			
			// modify the growth option to resolve incomplete states
			foreach(var resolver in resolvers)
				resolver.Apply(option);

			option.Apply( ps );
		}

		public abstract GrowthOption[] GetGrowthOptions();

	}

}