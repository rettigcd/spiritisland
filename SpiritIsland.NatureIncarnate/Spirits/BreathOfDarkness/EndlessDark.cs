namespace SpiritIsland.NatureIncarnate;

public static class EndlessDark {
	public const string SpaceLabel = "Endless Dark";
	static readonly public FakeSpace Space = new FakeSpace(SpaceLabel, GetLayout() ); // stores 'Abducted' tokens

	static SpaceLayout GetLayout() {
		//const float x = .0f;
		//const float y = .95f;
		//const float f = .3f;
		//return new SpaceLayout(
		//	new XY(x + 0, y + 0),
		//	new XY(x + f, y + 0),
		//	new XY(x + f, y - f),
		//	new XY(x + 0, y - f)
		//);

		const float x = .0f;
		const float y = .95f;
		const float f = .1f;
		return new SpaceLayout(
			new XY(x + 1*f, y - 0*f),
			new XY(x + 2*f, y - 0*f),
			new XY(x + 3*f, y - 1*f),
			new XY(x + 3*f, y - 2*f),
			new XY(x + 2*f, y - 3*f),
			new XY(x + 1*f, y - 3*f),
			new XY(x + 0*f, y - 2*f),
			new XY(x + 0*f, y - 1*f)
		);

	}

}