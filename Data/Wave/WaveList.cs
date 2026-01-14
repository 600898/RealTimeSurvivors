using RealTimeSurvivors.Components.Units;
namespace RealTimeSurvivors.Data.Wave
{
  /// <summary>
  /// Static List for defining every wave in the Game.
  /// Used by the WaveManager to spawn in entities.
  /// </summary>
  public class WaveList
  {
    public static readonly WaveDef[] Waves = new WaveDef[]
{
      new WaveDef(new UnitGroup[]
      {
        new UnitGroup(UnitType.Basic, 5)
      }),

      new WaveDef(new UnitGroup[]
      {
        new UnitGroup(UnitType.Basic, 10),
        new UnitGroup(UnitType.Ranged, 5)
      }),

      new WaveDef(new UnitGroup[]
      {
        new UnitGroup(UnitType.Basic, 20),
        new UnitGroup(UnitType.Ranged, 10)
      }),

      new WaveDef(new UnitGroup[]
      {
        new UnitGroup(UnitType.Basic, 40),
        new UnitGroup(UnitType.Ranged, 20)
      }),

      new WaveDef(new UnitGroup[]
      {
        new UnitGroup(UnitType.Basic, 60),
        new UnitGroup(UnitType.Ranged, 30)
      }),

};
  }
}
