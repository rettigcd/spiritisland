using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class ActionEngine {

		public Spirit Self { get; }

		public GameState GameState { get; }

		public PowerCardApi Api { get; }

		public ActionEngine(Spirit self,GameState gameState){
			Self = self;
			GameState = gameState;
			Api = new PowerCardApi();
			this.decisions = self.decisions;
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

		public async Task<int> SelectTextIndex( string prompt, params string[] options ) {
			string result = await SelectText(prompt,options);
			for(int i=0;i<options.Length;++i)
				if(result == options[i]) return i;
			return -1;
		}


		#endregion


	}

}
