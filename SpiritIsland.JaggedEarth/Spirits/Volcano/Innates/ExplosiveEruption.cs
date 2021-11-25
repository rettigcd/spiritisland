
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {


	[InnatePower("Explosive Eruption"), Fast, Erruption]
	public class ExplosiveEruption {

		[InnateOption("2 fire, 2 earth","2 destroyedpresence In one land withing range 1, X Damage",0)]
		static public async Task Option1(ErruptionCtx ctx ) {
			if(2 <= ctx.DestroyedPresence) {
				TargetSpaceCtx spaceCtx = await ctx.SelectAdjacentLandOrSelf($"Apply {ctx.DestroyedPresence} damage to");
				await spaceCtx.DamageInvaders(ctx.DestroyedPresence);
			}
		}

		[InnateOption("3 fire, 3 earth","4 destroyedpresence Generate X fear.",1)]
		static public Task Option2(ErruptionCtx ctx ) {
			if(2<=ctx.DestroyedPresence)
				ctx.AddFear( ctx.DestroyedPresence );
			return Task.CompletedTask;
		}

		[InnateOption("4 fire, 2 air, 4 earth","6 destroyedpresence In each land within range 1, 4 Damage.  Add 1 blight to target land; doing so does not Destroy your presence.",2)]
		static public Task Option3(ErruptionCtx ctx ) {
			if(6 <= ctx.DestroyedPresence) {
				// In each land within range 1,4 Damage.
				foreach(var adj in ctx.Adjacent) 
					ctx.Target(adj).DamageInvaders(4);

				// Add 1 blight to target land; doing so does not Destroy your presence.
				ctx.AddBlight(); // !!! when we fix adding blight, we need to make sure this incantation does not destroy presence
			}
			return Task.CompletedTask;
		}

		[InnateOption("5 fire, 3 air, 5 earth","10 destroyedpresence In each land withing range 2, +4 Damage.  In each land adjacent to the target, add 1 blight if it doesn't have any.",3)]
		static public async Task Option4(ErruptionCtx ctx ) {
			if(10 <= ctx.DestroyedPresence) {

				// In each land withing range 2
				// +4 Damage.
				foreach(var space in ctx.Range( 2 ))
					await ctx.Target(space).DamageInvaders(4);

				// In each land adjacent to the target
				// add 1 blight if it doesn't have any.
				foreach(var adj in ctx.Adjacent) {
					var adjCtx = ctx.Target(adj);
					if(!adjCtx.Blight.Any)
						await adjCtx.AddBlight();
				}
			}
		}

	}

	// Allows user to enter # of presence to destroy while generating the CTX
	class ErruptionAttribute : FromPresenceAttribute {
		public ErruptionAttribute() : base( 0 ) { }

		public override async Task<object> GetTargetCtx( string powerName, SpiritGameStateCtx ctx , TargettingFrom powerType ) {
			var target = (TargetSpaceCtx) await base.GetTargetCtx( powerName, ctx, powerType );

			int count = await target.Self.SelectNumber("# of presence to destroy?", target.Presence.Count,0);

			// Destroy them now
			for(int i=0;i<count;++i)
				await target.Presence.Destroy(target.Space);

			return new ErruptionCtx(target,count);
		}

	}

	public class ErruptionCtx : TargetSpaceCtx {
		public int DestroyedPresence { get; }
		public ErruptionCtx(TargetSpaceCtx orig, int count ) : base( orig, orig.Space ) { 
			DestroyedPresence = count; 
		}
	}

}
