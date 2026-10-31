namespace tests;

using Minlicon.Core.TileSystem;
using Xunit.Abstractions;

public class UnitTest1 {
    private readonly ITestOutputHelper output;

    public UnitTest1(ITestOutputHelper output) => this.output = output;

    [Fact]
    public void NewWorld_InitializesAllCells() {
        TileSystem world = new TileSystem(100, 100);
        // world.Set(10, 10, new TileCell(TileDefinition.Umad, 0));
        TileCell TestTile = world.Get(10, 10);
        Assert.Equal(TileDefinition.Umad, TestTile.TileType);
        Assert.Equal((byte)0, TestTile.Rotate);
        output.WriteLine(TestTile.ToString());
        output.WriteLine(TestTile.Rotate.ToString());
    }
}