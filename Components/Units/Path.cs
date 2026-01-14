using RealTimeSurvivors.Components.Shared;
using System.Linq;

namespace RealTimeSurvivors.Components.Units
{
  /// <summary>
  /// Component for keeping track of the Path a Unit follows during its Move cycle.
  /// </summary>
  public class Path
  {
    public Position[] Value;
    public Position Target;
    public int index;

    public Path(Position[] Value)
    {
      this.Value = Value;
      index = 0;
      if (Value.Length < 1)
      {
        Target = Value.Last();
      }
      else
      {
        Target.Value = (0, 0);
      }
    }
  }
}