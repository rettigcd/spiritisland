using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class Plaguebearers {

		[SpiritCard("Plaguebearers",1,Element.Fire,Element.Water,Element.Animal), Slow, FromPresence(2,Target.Disease)]
		static public async Task ActAsync(TargetSpaceCtx ctx ) {

			// 1 fear if invaders are present.
			if(ctx.HasInvaders)
				ctx.AddFear(1);

			// For each disease, Push 2 explorer / town / dahan.
			await ctx.Pusher
				.AddGroup( ctx.Disease.Count, Invader.Explorer, Invader.Town, TokenType.Dahan )
				.AddCustomMoveAction( async ( _, from, to ) => {
					// 1 disease may move with each Pushed piece.
					var option = new SpaceToken(from, TokenType.Disease);
					var diseaseToken = await ctx.Decision( Select.TokenFromManySpaces.ToGather(1,to,new[]{ option }, Present.Done) );
					if( diseaseToken != null )
						await ctx.Move(option.Token,option.Space,to);
				} )
				.MoveN();

		}

	}

}
