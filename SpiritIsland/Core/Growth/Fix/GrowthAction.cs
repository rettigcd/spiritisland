
namespace SpiritIsland.Core {

	public abstract class GrowthAction : IActionFactory, IAction {

		protected Spirit spirit;
		protected GameState gameState;

		protected GrowthAction(){}

		public abstract void Apply();

		public virtual string ShortDescription => this.ToString().Substring(13); // strip off SpiritIsland. prefix

		public virtual string Name => this.ShortDescription;

		public IAction Bind(Spirit spirit, GameState gameState) {
			this.spirit = spirit;
			this.gameState = gameState;
			return this;
		}

		public void Resolved(Spirit spirit){
			spirit.UnresolvedActionFactories.Remove(this);
			if(spirit.UnresolvedActionFactories.Count==0)
				spirit.CollectEnergy();
		}


		public abstract IOption[] Options {get;}

		public virtual void Select(IOption option) {
			// !!! once all growth actions implement this,
			// replace this with pure abstract
			throw new System.NotImplementedException();
		}

		public abstract bool IsResolved {get;}

		public Speed Speed => Speed.Growth;
	}

}
