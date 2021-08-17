using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class ActionEngine {

		public ActionEngine( Spirit self, GameState gameState ) {
			Self = self;
			GameState = gameState;
			this.decisions = self.decisions;
		}

		public Spirit Self { get; }

		public GameState GameState { get; }

		#region Spirit Configurable

		public Task<Space> TargetSpace( ActionEngine engine, From sourceEnum, int range, Target filterEnum )
			=> Self.TargetLandApi.TargetSpace(engine,sourceEnum,range,filterEnum);

		public Task<Space> TargetSpace( ActionEngine engine, From sourceEnum, Terrain? sourceTerrain, int range, Target filterEnum )
			=> Self.TargetLandApi.TargetSpace( engine, sourceEnum, sourceTerrain, range, filterEnum );

		public InvaderGroup InvadersOn(Space space) => Self.BuildInvaderGroup(GameState,space);

		public async Task DamageInvaders( Space space, int damage ) { // !!! let players choose the item to apply damage to
			if(damage == 0) return;
			await InvadersOn(space).ApplySmartDamageToGroup( damage );
		}


		#endregion

		public void Deconstruct(out Spirit self, out GameState gameState) {
	        self = Self;
			gameState = GameState;
		}

		#region basic selects

		readonly Stack<IDecision> decisions;

		public Task<Spirit> SelectSpirit() {
			var result = new TaskCompletionSource<Spirit>();

			decisions.Push( new SelectSpirit( GameState.Spirits
				, spirit => result.TrySetResult( spirit )
			) );

			return result.Task;
		}

		public Task<Space> SelectSpace( string prompt, IEnumerable<Space> spaces, bool allowShortCircuit = false ) {
			var result = new TaskCompletionSource<Space>();
			decisions.Push( new SelectAsync<Space>( prompt, spaces.ToArray(), allowShortCircuit, result ) );
			return result.Task;
		}

		public Task<InvaderSpecific> SelectInvader( string prompt ,InvaderSpecific[] invaders ,bool allowShortCircuit=false) {
			var result = new TaskCompletionSource<InvaderSpecific>();
			decisions.Push( new SelectAsync<InvaderSpecific>( 
				prompt, 
				invaders,
				allowShortCircuit,
				result 
			));
			return result.Task;
		}

		public Task<IOption> SelectOption( string prompt ,IOption[] options ,bool allowShortCircuit=false) {
			var result = new TaskCompletionSource<IOption>();
			decisions.Push( new SelectAsync<IOption>( 
				prompt, 
				options,
				allowShortCircuit,
				result 
			));
			return result.Task;
		}

		public Task<IActionFactory> SelectFactory( string prompt,IActionFactory[] options,bool allowShortCircuit=false) {
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
				Self.Presence.GetPlaceableFromTracks(),
				false,
				result
			) );
			return result.Task;
		}

		public Task<string> SelectText( string prompt,params string[] options) {
			var result = new TaskCompletionSource<string>();
			decisions.Push( new SelectTextAsync(prompt, options,result));
			return result.Task;
		}

		public async Task<bool> SelectFirstText( string prompt, string option1, string option2 ) {
			return await SelectText(prompt,option1,option2) == option1;
		}

		#endregion


	}

}
