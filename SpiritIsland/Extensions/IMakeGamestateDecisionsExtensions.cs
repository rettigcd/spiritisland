using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	// High-Level game actions
	public static class IMakeGamestateDecisionsExtensions {

		static public SpiritGameStateCtx MakeDecisionsFor(this Spirit spirit, GameState gameState ) 
			=> new SpiritGameStateCtx( spirit, gameState );

		#region extends spirits - Select Action

		static public async Task SelectAction( this Spirit spirit, string prompt, params ActionOption[] options ) {
			var applicable = options.Where( opt => opt.IsApplicable ).ToArray();
			string text = await spirit.SelectText( prompt, applicable.Select( a => a.Description ).ToArray() );
			if(text != null) {
				var selectedOption = applicable.Single( a => a.Description == text );
				await selectedOption.Action();
			}
		}

		static public async Task SelectOptionalAction( this Spirit spirit, string prompt, params ActionOption[] options ) {
			var applicable = options.Where( opt => opt.IsApplicable ).ToArray();
			string text = await spirit.SelectText( prompt, applicable.Select( a => a.Description ).ToArray(), Present.Done );
			if(text != null) {
				var selectedOption = applicable.Single( a => a.Description == text );
				await selectedOption.Action();
			}
		}

		#endregion

	}

}