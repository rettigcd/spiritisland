using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class ActionEngine {

		public ActionEngine( Spirit self, GameState gameState ) {
			Self = self;
			GameState = gameState;
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

		public void AddFear( int count) {
			GameState.AddFearDirect( count );
		}

	}

	static public class SpiritDecisionExtensinos {

		static public Task<Spirit> SelectSpirit(this ActionEngine eng, Spirit[] spirits) {
			var result = new TaskCompletionSource<Spirit>();

			eng.Self.decisions.Push( new SelectSpirit( spirits
				, spirit => result.TrySetResult( spirit )
			) );

			return result.Task;
		}

		static public Task<Space> SelectSpace( this ActionEngine eng, string prompt, IEnumerable<Space> spaces, bool allowShortCircuit = false ) {
			var result = new TaskCompletionSource<Space>();
			eng.Self.decisions.Push( new SelectAsync<Space>( prompt, spaces.ToArray(), allowShortCircuit, result ) );
			return result.Task;
		}

		static public Task<InvaderSpecific> SelectInvader( this ActionEngine eng, string prompt, InvaderSpecific[] invaders, bool allowShortCircuit = false ) {
			var result = new TaskCompletionSource<InvaderSpecific>();
			eng.Self.decisions.Push( new SelectAsync<InvaderSpecific>(
				prompt,
				invaders,
				allowShortCircuit,
				result
			) );
			return result.Task;
		}

		static public Task<IOption> SelectOption( this ActionEngine eng, string prompt, IOption[] options, bool allowShortCircuit = false ) {
			var result = new TaskCompletionSource<IOption>();
			eng.Self.decisions.Push( new SelectAsync<IOption>(
				prompt,
				options,
				allowShortCircuit,
				result
			) );
			return result.Task;
		}

		static public Task<IActionFactory> SelectFactory( this ActionEngine eng, string prompt, IActionFactory[] options, bool allowShortCircuit = false ) {
			var result = new TaskCompletionSource<IActionFactory>();
			eng.Self.decisions.Push( new SelectAsync<IActionFactory>(
				prompt,
				options,
				allowShortCircuit,
				result
			) );
			return result.Task;
		}

		static public Task<Track> SelectTrack( this ActionEngine eng ) {
			var result = new TaskCompletionSource<Track>();

			eng.Self.decisions.Push( new SelectAsync<Track>(
				"Select Presence to place.",
				eng.Self.Presence.GetPlaceableFromTracks(),
				false,
				result
			) );
			return result.Task;
		}

		static public Task<string> SelectText( this ActionEngine eng, string prompt, params string[] options ) {
			var result = new TaskCompletionSource<string>();
			eng.Self.decisions.Push( new SelectTextAsync( prompt, options, result ) );
			return result.Task;
		}

		static public async Task<bool> SelectFirstText( this ActionEngine eng, string prompt, string option1, string option2 ) {
			return await eng.SelectText( prompt, option1, option2 ) == option1;
		}

	}

}