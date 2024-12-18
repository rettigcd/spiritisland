namespace SpiritIsland.Tests.Core; 

public class Targeting_Tests {

	[Fact]
	public async Task MultipleOrigins_TargetOnlyShowsActualOrigin() {
		var gs = new SoloGameState();
		// Given: Presence on A3 & A8
		gs.Board[3].Given_InitSummary("1TS");
		gs.Board[8].Given_InitSummary("1TS");

		// Given: has the ActionScope Container
		Type type = typeof(ActionScope);
		FieldInfo info = type.GetField("_scopeContainer", BindingFlags.NonPublic | BindingFlags.Static);
		var holder = (AsyncLocal <ActionScope.ActionScopeContainer>)info.GetValue(null);
		ActionScope.ActionScopeContainer container = holder.Value;

		ActionScope scope1 = container.Current;

		await using ActionScope scope = await ActionScope.StartSpiritAction(ActionCategory.Spirit_Power, gs.Spirit);

		ActionScope scope2 = container.Current;
		(scope1 == scope2).ShouldBeFalse();

		// When: playing a card at range-0
		await gs.Spirit.When_ResolvingCard<CaptureTargetSources>(user => {
			user.NextDecision.Choose("A3");
		});

		CaptureTargetSources.Origins.Select(x => x.Label).OrderBy(x => x).Join(",").ShouldBe("A3");
	}

	public class CaptureTargetSources {
		[MinorCard("TestCard", 1, "fire"), Fast, FromPresence(0)]
		[Instructions("..."), Artist(Artists.JoshuaWright)]
		static public Task ActAsync(TargetSpaceCtx _) {
			Origins = TargetSpaceAttribute.TargettedSpace.Sources;
			return Task.CompletedTask;
		}
		static public Space[] Origins { get; set; }
	}

}
