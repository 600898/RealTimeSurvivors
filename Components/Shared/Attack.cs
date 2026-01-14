namespace RealTimeSurvivors.Components.Shared
{
  /// <summary>
  /// Component for entities with an attack.
  /// Example: BuildingTypes.Tower & UnitTypes.Basic
  /// </summary>
  public struct Attack
  {
    public float Range;
    public float Damage;
    public float Cooldown;
    public float CurrentCooldown;
    public int TargetId;

    public Attack(float Range, float Damage, float AttackCooldown)
    {
      this.Range = Range;
      this.Damage = Damage;
      Cooldown = AttackCooldown;
      CurrentCooldown = AttackCooldown;
      TargetId = -1;
    }
  }
}
