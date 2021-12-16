using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	class ManyMindsPresence : SpiritPresence {

		ManyMindsMoveAsOne spirit;

		public ManyMindsPresence(PresenceTrack energy, PresenceTrack cards ) : base( energy, cards ) {}

		public void Watch(GameState gs,ManyMindsMoveAsOne spirit) {
			this.spirit = spirit; 
			gs.Tokens.TokenMoved.ForGame.Add( TokenMoved );
			gs.Tokens.TokenDestroyed.ForGame.Add( TokenDestroyed );
		}

		public override void PlaceOn( Space space, GameState gs ) {
			base.PlaceOn( space, gs );
			// if created sacred site, create virtual beast
			if(CountOn( space ) == 2)
				gs.Tokens[space].Beasts.Add(1); // Beast token is virtual so maybe we don't want to trigger TokenAdded
		}

		protected override void RemoveFrom_NoCheck( Space space, GameState gs ) {
			base.RemoveFrom_NoCheck( space, gs );
			// if destroyed sacred site, remove virtual beast
			if( CountOn( space ) == 1 )
				gs.Tokens[space].Beasts.Remove(1);
		}

		async Task TokenMoved(GameState gs, TokenMovedArgs args) {
			if(args.Token != TokenType.Beast) return; // not a beast
			if(this.CountOn( args.From ) < 2) return; // not our Sacred Site


			var srcBeasts = gs.Tokens[args.From].Beasts;
			if(srcBeasts.Count > 0 // force moved our virtual beast
				&& await spirit.Action.Decision( Select.DeployedPresence.Gather( "Move 2 presence with Beast?", args.To, new []{ args.From } ) ) == null
			) return; // not moving presence

			Move2Presence( gs, args );

			SacredSiteAtSouce_RestoreVirtualBeast( args, srcBeasts );

			AddedVirtualBeastAtDestination_LimitTo1( gs, args );

		}

		void Move2Presence( GameState gs, TokenMovedArgs args ) {
			// Move 2 of our presence
			for(int i = 0; i < 2; ++i) {
				base.RemoveFrom_NoCheck( args.From, gs ); // using base because we don't want to trigger anything
				base.PlaceOn( args.To, gs );
			}
		}

		void SacredSiteAtSouce_RestoreVirtualBeast( TokenMovedArgs args, TokenBinding srcBeasts ) {
			if(2 <= CountOn( args.From ))
				srcBeasts.Add(1); // Beast token is virtual so maybe we don't want to trigger TokenAdded
		}

		void AddedVirtualBeastAtDestination_LimitTo1( GameState gs, TokenMovedArgs args ) {
			// if destination/to now has 4 or more presence,
			// then there was already a virtual beast there and we need to remove 1 of the virtual beasts
			if(4 <= CountOn( args.To ))
				gs.Tokens[args.To].Beasts.Remove(1);
		}

		async Task TokenDestroyed(GameState gs, TokenDestroyedArgs args) {
			if(args.Token != TokenType.Beast.Generic) return; // not a beast
			if(this.CountOn(args.Space)<2) return; // not our Sacred Site
			if(gs.Tokens[args.Space].Beasts.Count == 0){ // no more beasts
				await base.Destroy(args.Space,gs, args.Cause);
				await base.Destroy(args.Space,gs, args.Cause);
			}
		}

	}

}
