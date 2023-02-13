namespace SpiritIsland.JaggedEarth;

public partial class ManyMindsMoveAsOne {
	class ManyMindTokens : SpaceState {
		public ManyMindTokens( SpaceState src ):base( src ) { }
		public override TokenGatherer Gather( Spirit self ) => new BeastGatherer(self,this);
		public override TokenPusher Pusher( Spirit self ) => new BeastPusher(self,this);
	}

}