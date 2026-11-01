namespace tests;

using Minlicon.Core.TileSystem;
using Xunit.Abstractions;

public class UnitTest1 {
    private readonly ITestOutputHelper output;

    public UnitTest1(ITestOutputHelper output) => this.output = output;

    [Fact]
    public void NewWorld_InitializesAllCells() {
        TileSystem world = new TileSystem(100, 100);

        TileCell TestTile = world.Get(10, 10);
    
        Assert.Equal(TileGround.Grass, world.Get(10, 10).TileGroundType);

        output.WriteLine(TestTile.ToString());
    }
}