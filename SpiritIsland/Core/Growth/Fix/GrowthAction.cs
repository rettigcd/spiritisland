
namespace SpiritIsland.Core {

	public class GrowthAction : IActionFactory, IAction {

		protected Spirit spirit;
		protected GameState gameState;

		protected GrowthAction(){}

		public virtual void Apply(){ throw new System.NotImplementedException(); }

		public virtual string ShortDescription => this.ToString().Substring(13); // strip off SpiritIsland. prefix

		public virtual string Name => this.ShortDescription;

		public virtual IAction Bind(Spirit spirit, GameState gameState) {
			this.spirit = spirit;
			this.gameState = gameState;
			return this;
		}

		public void Resolved(Spirit spirit){
			spirit.UnresolvedActionFactories.Remove(this);
			if(spirit.UnresolvedActionFactories.Count==0)
				spirit.CollectEnergy();
		}


		public virtual IOption[] Options => throw new System.NotImplementedException(); 

		public virtual void Select(IOption option) {
			// !!! once all growth actions implement this,
			// replace this with pure abstract
			throw new System.NotImplementedException();
		}

		public virtual bool IsResolved => throw new System.NotImplementedException(); 

		public Speed Speed => Speed.Growth;
	}

}
