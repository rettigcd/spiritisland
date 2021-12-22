using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.PromoPack1 {
	[InnatePower( "Firestorm" ),Fast]
	[FromPresence(0,Target.Blight)]
	public class FireStorm {

		// Group 0
		[InnateOption( "1 plant", "1 Damage per 2 fire you have.", 0 )]
		static public Task Option1( TargetSpaceCtx ctx ) {
			int fireDamage = ctx.Self.Elements[Element.Fire] / 2; // rounding down
			return DoFireDamage( ctx, fireDamage );
		}

		[InnateOption( "3 plant", "Instead, 1 Damage per fire you have.", 0 )]
		static public Task Option2( TargetSpaceCtx ctx ) {
			int fireDamage = ctx.Self.Elements[Element.Fire];
			return DoFireDamage( ctx, fireDamage );
		}

		static async Task DoFireDamage( TargetSpaceCtx ctx, int fireDamage ) {
			if( fireDamage == 0) return;
			if( await CanSplitDamage(ctx) ) 
				await DoFireDamageToMultipleTargets( ctx, fireDamage );
			else
				await ctx.DamageInvaders( fireDamage );
		}

		private static async Task DoFireDamageToMultipleTargets( TargetSpaceCtx ctx, int fireDamage ) {
			await ctx.DamageInvaders( 1 ); // because they targetted a land, only select invaders from that land.
			--fireDamage;

			var spacesWithPresenceAndBlight = ctx.Self.Presence.Spaces
				.Select(s=>ctx.Target(s))
				.Where( x=>x.HasBlight )
				.ToArray();
			// ! don't .ToArray() this because we want it to re-execute each time.
			var tokens = spacesWithPresenceAndBlight
				.SelectMany( ctx => ctx.Tokens.Keys.Select(t=>new SpaceToken(ctx.Space,t)));

			while(fireDamage > 0 && tokens.Any()) {
				var token = await ctx.Decision( new Select.TokenFromManySpaces($"Apply fire damage. ({fireDamage} remaining)",tokens,Present.Always));
				await ctx.Target(token.Space).Invaders.ApplyDamageTo1(1,token.Token);
				--fireDamage;
			}
		}


		[InnateOption( MultiTargetThreshold, "You may split this Power's damage among any number of lands with blight where you have presence.", AttributePurpose.DisplayOnly, 0 )]
		static Task<bool> CanSplitDamage(TargetSpaceCtx ctx) => ctx.YouHave( MultiTargetThreshold );
		const string MultiTargetThreshold = "4 fire,2 air";

		// Group 1
		[InnateOption( "7 fire", "In a land with blight where you have presence, Push all dahan.  Destory all Invaders and beast. 1 blight.", 1 )]
		static public async Task Option4( TargetSpaceCtx ctx ) {
			// In a land with blight and presence  (Select a space, not necessarily the one you targetted with power (I guess...)
			var spacesWithPresenceAndBlight = ctx.Self.Presence.Spaces
				.Where( s=>ctx.Target(s).HasBlight );
			var space = await ctx.Decision( new Select.Space($"Push all dahan, destroy invaders and beast, 1 blight",spacesWithPresenceAndBlight,Present.Always));
			var spaceCtx = ctx.Target( space );

			// Push all Dahan
			await spaceCtx.PushDahan( int.MaxValue );

			// Destory all invaders and Beasts
			var beasts = spaceCtx.Beasts;
			await beasts.Destroy( beasts.Count );
			await spaceCtx.Invaders.DestroyAny(int.MaxValue,Invader.Explorer,Invader.Town,Invader.City);

			// Add 1 blight
			await ctx.AddBlight();
			
		}

	}

}
