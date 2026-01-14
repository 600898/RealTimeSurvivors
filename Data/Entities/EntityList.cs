using RealTimeSurvivors.Components.Buildings;
using RealTimeSurvivors.Components.Shared;
using RealTimeSurvivors.Components.Units;
using System.Collections.Generic;
namespace RealTimeSurvivors.Data.Entities
{
  /// <summary>
  /// Static Lists for getting Building & Unit definitions.
  /// </summary>
  public static class EntityList
  {

    public static readonly Dictionary<BuildingType, Building> Buildings = new Dictionary<BuildingType, Building>()
    {
      /*************************************************************************************************
      *                                                    Type                Size     Health Cost    *
      *************************************************************************************************/                                                    
      {BuildingType.Heart ,      new Building(BuildingType.Heart,     new Size(10, 10), 10000,   100000) },
      {BuildingType.Generator ,  new Building(BuildingType.Generator, new Size(4, 4),   150,    200) },
      {BuildingType.Tower ,      new Building(BuildingType.Tower,     new Size(4, 4),   500,    750) },
      {BuildingType.Wall ,       new Building(BuildingType.Wall,      new Size(4, 4),   500,    100) },
      {BuildingType.Forge ,      new Building(BuildingType.Forge,     new Size(4, 4),   1000,   500) }
    };


    public static readonly Dictionary<UnitType, Unit> Units = new Dictionary<UnitType, Unit>()
    {
      /******************************************************************************************************************
      *                             UnitType                   Size     Health             Attack       Speed   Cost    *
      ******************************************************************************************************************/
      {UnitType.Basic ,    new Unit(UnitType.Basic,   new Size(1, 1),   100,    new Attack(16, 25, 4),  100f,   150) },
      {UnitType.Ranged ,   new Unit(UnitType.Ranged,  new Size(1, 1),   50,     new Attack(128, 10, 2), 100f,   150) }
    };
  }
}
