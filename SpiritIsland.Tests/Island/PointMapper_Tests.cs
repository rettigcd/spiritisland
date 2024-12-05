namespace SpiritIsland.Tests;

public class PointMapper_Tests {


	[Fact]
	public void CanCreateRotatedMapper() {

		// Given: some rotated points
		XY[] originalPoints = [ new XY(0,0), new XY(10,0), new XY(10,5), new XY(0,5)];
		XY[] rotated = originalPoints.Select(new PointMapper(RowVector.RotateDegrees(-30)*RowVector.Translate(5,5)).Map).ToArray();
		rotated[0].ShouldBe(5f,5f); // make sure the rotation happened first, then the translation

		//  And: a viewport with the same dimmensions
		Bounds viewportRect = new Bounds(33, 22, 10, 5);

		// When: we try to find a mapper that fits them into the view port
		var mapper = PointMapper.FitPointsInViewportHeight(viewportRect, rotated, 30, 60, 90, 120, 150);

		// Then: Bounds should roughly match original
		var bounds = BoundsBuilder.ForPoints(rotated.Select(mapper.Map));
		// Bounds width should be close to viewport width
		// Bounds height should be close to viewport height
	}

}