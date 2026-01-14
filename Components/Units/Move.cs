using RealTimeSurvivors.Components.Shared;

namespace RealTimeSurvivors.Components.Units
{
  /// <summary>
  /// Component for representing a movable unit's next movement.
  /// </summary>
  public struct Move
  {
    public Position position;
    public MoveType type;
    public bool consumeOnArrival;

    public Move(Position position, MoveType type = MoveType.Direct, bool consumeOnArrival = true)
    {
      this.position = position;
      this.type = type;
      this.consumeOnArrival = consumeOnArrival;
    }
  }
}
