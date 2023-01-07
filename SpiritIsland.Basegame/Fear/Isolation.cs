namespace SpiritIsland.Basegame;

public class Isolation : FearCardBase, IFearCard {

	public const string Name = "Isolation";
	public string Text => Name;

	[FearLevel( 1, "Each player removes 1 Explorer / Town from a land where it is the only Invader." )]
	public Task Level1( GameCtx ctx ) {
		return RemoveInvaderWhenMax( ctx, 1, Invader.Explorer_Town );
	}

	[FearLevel( 2, "Each player removes 1 Explorer / Town from a land with 2 or fewer Invaders." )]
	public Task Level2( GameCtx ctx ) {
		return RemoveInvaderWhenMax( ctx, 2, Invader.Explorer_Town );
	}

	[FearLevel( 3, "Each player removes an Invader from a land with 2 or fewer Invaders." )]
	public Task Level3( GameCtx ctx ) {
		return RemoveInvaderWhenMax( ctx, 2, Invader.Any );
	}

	/// <summary>
	/// Conditionally Removes an invader based on the Total # of invaders in a space
	/// </summary>
	static async Task RemoveInvaderWhenMax( GameCtx ctx, int invaderMax, params TokenClass[] removeableInvaders ) {
		foreach(var spirit in ctx.Spirits) {

			var options = spirit.GameState.AllActiveSpaces
				.Where( s => s.HasAny( removeableInvaders ) && s.InvaderTotal() <= invaderMax )
				.Select( s => s.Space )
				.ToArray();
			if(options.Length == 0) return;

			await spirit.RemoveTokenFromOneSpace(options,1,removeableInvaders);
		}
	}

}