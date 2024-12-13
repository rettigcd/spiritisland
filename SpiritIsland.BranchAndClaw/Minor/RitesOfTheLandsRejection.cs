namespace SpiritIsland.BranchAndClaw;

public class RitesOfTheLandsRejection {

	public const string Name = "Rites of the Land's Rejection";

	[MinorCard( RitesOfTheLandsRejection.Name, 1, Element.Moon, Element.Fire, Element.Earth ), Fast, FromSacredSite( 2, Filter.Dahan )]
	[Instructions( "Invaders do not Build in target land this turn. 1 Fear per Town / City or 1 Fear per Dahan, whichever is less. -or- Push up to 3 Dahan." ), Artist( Artists.JoshuaWright )]
	static public Task ActAsync( TargetSpaceCtx ctx ) {

		static Task StopBuild_FearForCitiesTownsAndDahan(TargetSpaceCtx ctx) {
			// Invaders Do not build in target land this turn
			ctx.Space.SkipAllBuilds( Name );

			// 1 fear per town/city OR 1 fear per dahan, whichever is less
			int cityTownCount = ctx.Space.SumAny( Human.Town_City );
			return ctx.AddFear( Math.Min( ctx.Dahan.CountAll, cityTownCount ) );
		}

		return ctx.SelectActionOption(
			new SpaceAction( "Stop build - 1 fear / (Dahan or T/C)", StopBuild_FearForCitiesTownsAndDahan ),
			Cmd.PushUpToNDahan(3)
		);
	}

}