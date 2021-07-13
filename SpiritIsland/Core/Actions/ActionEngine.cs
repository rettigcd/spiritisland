using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Core {

	public class ActionEngine {

		readonly Stack<IDecision> decisions;
		public Spirit Self { get; }
		public GameState GameState { get; }

		public ActionEngine(Spirit self,GameState gameState,Stack<IDecision> decisions){
			Self = self;
			GameState = gameState;
			this.decisions = decisions;
		}

		public void Deconstruct(out Spirit self, out GameState gameState) {
	        self = Self;
			gameState = GameState;
		}



		public Task<Spirit> SelectSpirit( Spirit[] spirits ) {
			var result = new TaskCompletionSource<Spirit>();

			decisions.Push( new SelectSpirit( spirits
				, spirit => result.TrySetResult( spirit )
			) );

			return result.Task;
		}

		public Task<Space> SelectSpace(
			string prompt,
			IEnumerable<Space> spaces,
			bool allowShortCircuit = false
		) {
			var result = new TaskCompletionSource<Space>();
			decisions.Push( new SelectAsync<Space>( prompt, spaces.ToArray(), allowShortCircuit, result ) );
			return result.Task;
		}

		#region Target Helpers

		public Task<Spirit> TargetSpirit()
			=> SelectSpirit(GameState.Spirits);

		public async Task<Space> TargetSpace_Presence(int range){
			return await SelectSpace("Select target.",Self.Presence.Range(range));
		}
		public async Task<Space> TargetSpace_Presence(int range, Func<Space,bool> filter){
			return await SelectSpace("Select target.",Self.Presence.Range(range).Where(filter));
		}

		public async Task<Space> TargetSpace_SacredSite(int range){
			return await SelectSpace("Select target.",Self.SacredSites.Range(range));
		}
		public async Task<Space> TargetSpace_SacredSite(int range, Func<Space,bool> filter){
			return await SelectSpace("Select target.",Self.SacredSites.Range(range).Where(filter));
		}
		#endregion

		public Task<Invader> SelectInvader( string prompt
			,Invader[] invaders
			,bool allowShortCircuit=false
		) {
			var result = new TaskCompletionSource<Invader>();
			decisions.Push( new SelectAsync<Invader>( 
				prompt, 
				invaders,
				allowShortCircuit,
				result 
			));
			return result.Task;
		}

		public Task<IActionFactory> SelectFactory( string prompt
			,IActionFactory[] options
			,bool allowShortCircuit=false
		) {
			var result = new TaskCompletionSource<IActionFactory>();
			decisions.Push( new SelectAsync<IActionFactory>( 
				prompt, 
				options,
				allowShortCircuit,
				result 
			));
			return result.Task;
		}

		public Task<Track> SelectTrack() {
			var result = new TaskCompletionSource<Track>();
			decisions.Push( new SelectAsync<Track>( 
				"Select Presence to place.", 
				new Track[]{Track.Energy,Track.Card},
				false,
				result 
			));
			return result.Task;
		}

		public Task<string> SelectText( string prompt
			,params string[] options
		) {
			var result = new TaskCompletionSource<string>();
			decisions.Push( new SelectTextAsync(prompt, options,result));
			return result.Task;
		}








	}

}
