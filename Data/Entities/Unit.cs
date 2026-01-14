using RealTimeSurvivors.Components.Shared;
using RealTimeSurvivors.Components.Units;

namespace RealTimeSurvivors.Data.Entities
{
  /// <summary>
  /// Entity definition for a Unit.
  /// </summary>
  public struct Unit
  {
    public UnitType type;
    public Size size;
    public Health health;
    public Attack attack;
    public float speed;
    public int cost;

    public Unit(UnitType type, Size size, int health, Attack attack, float speed, int cost)
    {
      this.type = type;
      this.size = size;
      this.health = new Health { Value = health };
      this.attack = attack;
      this.speed = speed;
      this.cost = cost;
    }
  }
}
