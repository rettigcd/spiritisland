﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Core {

	public class ActionEngine {

		public Spirit Self { get; }

		public GameState GameState { get; }

		public PowerCardApi Api { get; }

		public ActionEngine(Spirit self,GameState gameState,Stack<IDecision> decisions){
			Self = self;
			GameState = gameState;
			Api = new PowerCardApi(this); // circular dependency
			this.decisions = decisions;
		}

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

		public Task<Invader> SelectInvader( string prompt ,Invader[] invaders ,bool allowShortCircuit=false) {
			var result = new TaskCompletionSource<Invader>();
			decisions.Push( new SelectAsync<Invader>( 
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
				new Track[]{Track.Energy,Track.Card},
				false,
				result 
			));
			return result.Task;
		}

		public Task<string> SelectText( string prompt,params string[] options) {
			var result = new TaskCompletionSource<string>();
			decisions.Push( new SelectTextAsync(prompt, options,result));
			return result.Task;
		}


		#endregion


	}

}
