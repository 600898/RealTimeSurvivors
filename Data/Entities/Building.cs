using RealTimeSurvivors.Components.Buildings;
using RealTimeSurvivors.Components.Shared;

namespace RealTimeSurvivors.Data.Entities
{
  /// <summary>
  /// Entity definition for a Building.
  /// </summary>
  public struct Building
  {
    public BuildingType type;
    public Size size;
    public int health;
    public int cost;

    public Building(BuildingType type, Size size, int health, int cost)
    {
      this.type = type;
      this.size = size;
      this.health = health;
      this.cost = cost;
    }
  }
}
