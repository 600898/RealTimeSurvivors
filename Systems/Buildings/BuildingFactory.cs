using RealTimeSurvivors.Components.Buildings;
using RealTimeSurvivors.Components.Shared;
using RealTimeSurvivors.Components.Units;
using RealTimeSurvivors.Systems.Shared;

namespace RealTimeSurvivors.Systems.Buildings
{
  /// <summary>
  /// Used to keep track of generic variables/settings for each building type, and functions to Create and destroy their specific components.
  /// </summary>
  public static class BuildingFactory
  {
    /*
    * Production component
    * Production(UnitTypes[], BaseCreationSpeed)
    */
    private static UnitType[] forgeUnitTypes = { UnitType.Basic, UnitType.Ranged };
    public static Production ForgeVars = new Production(forgeUnitTypes, 5, 1f);

    /*
    * Generator component
    */
    public static Generator GeneratorVars = new Generator()
    {
      TotalResources = 0.0f,
      ResourcePerGoal = 10.0f,
      DeltaGoal = 5.0f,
      CurrentDelta = 0.0f
    };

    /*
    * Heart component
    */
    public static Generator HeartResourcesVars = new Generator()
    {
      TotalResources = 0.0f,
      ResourcePerGoal = 100.0f,
      DeltaGoal = 10.0f,
      CurrentDelta = 0.0f
    };

    /*
    * Tower component
    * Attack(Range, Damage, AttackCooldown)
    */
    public static Attack TowerAttackVars = new Attack(288f, 25f, 2f) { };

    /// <summary>
    /// Assigns the correct components to the entity managers collections based on the BuildingTypes provided.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="_entities"></param>
    /// <param name="entity"></param>
    public static void CreateComponent(BuildingType type, EntityManager _entities, int entity)
    {
      switch (type)
      {
        case BuildingType.Forge:
          _entities.Productions[entity] = ForgeVars;
          break;
        case BuildingType.Generator:
          _entities.ResourceGenerators[entity] = GeneratorVars;
          break;
        case BuildingType.Heart:
          _entities.ResourceGenerators[entity] = HeartResourcesVars;
          break;
        case BuildingType.Tower:
          _entities.Attacks[entity] = TowerAttackVars;
          _entities.States[entity] = UnitState.Idle;
          _entities.hasAttack[entity] = true;
          break;
        case BuildingType.Wall:

          break;
      }
    }
    /// <summary>
    /// Removes the components from the entity manager based on the BuildingTypes provided.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="_entities"></param>
    /// <param name="entity"></param>
    public static void DestroyComponent(BuildingType type, EntityManager _entities, int entity)
    {
      switch (type)
      {
        case BuildingType.Forge:
          _entities.Productions.Remove(entity);
          break;
        case BuildingType.Generator:
          _entities.ResourceGenerators.Remove(entity);
          break;
        case BuildingType.Heart:
          _entities.ResourceGenerators.Remove(entity);
          break;
        case BuildingType.Tower:
          _entities.hasAttack[entity] = false;
          _entities.States[entity] = UnitState.None;
          break;
      }
    }
  }
}