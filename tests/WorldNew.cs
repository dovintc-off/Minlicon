namespace tests;

using Minlicon.Core.TileSystem;
using Xunit;
using Xunit.Abstractions;

public class UnitTest1 {
    private readonly ITestOutputHelper output;

    public UnitTest1(ITestOutputHelper output) => this.output = output;

    [Fact]
    public void NewWorld_InitializesAllCells() {
        GroundDefinition defaultGround = new GroundDefinition {
            Name = "Grass",
            CanBuild = true
        };

        TileSystem world = new TileSystem(100, defaultGround);

        TileCell testTile = world.Get(10, 10);

        Assert.Same(defaultGround, testTile.Ground);
        Assert.Null(testTile.Building);

        output.WriteLine(testTile.ToString());
    }
}