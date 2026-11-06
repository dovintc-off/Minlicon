namespace tests;

using Minlicon.Core.TileSystem;
using Xunit;

public class UnitTest2 {
    private static GroundDefinition CreateGround() {
        return new GroundDefinition {
            Name = "Grass",
            CanBuild = true
        };
    }

    [Fact]
    public void PlaceBuilding_Success() {
        TileSystem world = new TileSystem(100, CreateGround());

        BuildingInstance building = new BuildingInstance(
            new BuildingDefinition("drill", 2, 2, 100),
            10, 10
        );

        Assert.True(world.PlaceBuilding(building));

        Assert.Same(building, world.Get(10, 10).Building);
        Assert.Same(building, world.Get(11, 10).Building);
        Assert.Same(building, world.Get(10, 11).Building);
        Assert.Same(building, world.Get(11, 11).Building);
    }

    [Fact]
    public void PlaceBuilding_Occupied() {
        TileSystem world = new TileSystem(100, CreateGround());

        BuildingInstance first = new BuildingInstance(
            new BuildingDefinition("drill", 2, 2, 100),
            10, 10
        );

        BuildingInstance second = new BuildingInstance(
            new BuildingDefinition("storage", 2, 2, 100),
            11, 11
        );

        Assert.True(world.PlaceBuilding(first));
        Assert.False(world.PlaceBuilding(second));

        Assert.Same(first, world.Get(10, 10).Building);
    }

    [Fact]
    public void CanRemove_ValidBuilding() {
        TileSystem world = new TileSystem(100, CreateGround());

        BuildingInstance building = new BuildingInstance(
            new BuildingDefinition("drill", 2, 2, 100),
            10, 10
        );

        Assert.True(world.PlaceBuilding(building));

        Assert.True(world.CanRemove(10, 10));
        Assert.True(world.CanRemove(11, 11));
    }

    [Fact]
    public void RemoveBuilding_Success() {
        TileSystem world = new TileSystem(100, CreateGround());

        BuildingInstance building = new BuildingInstance(
            new BuildingDefinition("drill", 2, 2, 100),
            10, 10
        );

        Assert.True(world.PlaceBuilding(building));
        Assert.True(world.RemoveBuilding(11, 11));

        Assert.Null(world.Get(10, 10).Building);
        Assert.Null(world.Get(11, 10).Building);
        Assert.Null(world.Get(10, 11).Building);
        Assert.Null(world.Get(11, 11).Building);
    }

    [Fact]
    public void RemoveBuilding_PreservesGround() {
        TileSystem world = new TileSystem(100, CreateGround());

        GroundDefinition ground = world.Get(10, 10).Ground;

        BuildingInstance building = new BuildingInstance(
            new BuildingDefinition("drill", 2, 2, 100),
            10, 10
        );

        Assert.True(world.PlaceBuilding(building));
        Assert.True(world.RemoveBuilding(10, 10));

        Assert.Same(ground, world.Get(10, 10).Ground);
        Assert.Null(world.Get(10, 10).Building);
    }
}