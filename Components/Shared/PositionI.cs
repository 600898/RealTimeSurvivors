using System;
using Godot;

namespace RealTimeSurvivors.Components.Shared
{
  /// <summary>
  /// Integer representation of a Position.
  /// Used for representing an entity's position in Grid Space.
  /// </summary>
  public struct PositionI : IEquatable<PositionI>
  {
    public (int x, int y) Value;
    public int X => Value.x;
    public int Y => Value.y;

    public PositionI(int x, int y)
    {
      Value = (x, y);
    }

    public PositionI(float x, float y)
    {
      Value = ((int)x, (int)y);
    }

    public float DistanceTo(Position to)
    {
      return (float)Math.Sqrt((X - to.X) * (X - to.X) + (Y - to.Y) * (Y - to.Y));
    }

    public static explicit operator Position(PositionI p) { return new Position(p.X, p.Y); }
    public static explicit operator Vector2(PositionI p) { return new Vector2(p.X, p.Y); }

    public static PositionI operator +(PositionI left, PositionI right) { return new PositionI(left.X + right.X, left.Y + right.Y); }
    public static PositionI operator -(PositionI left, PositionI right) { return new PositionI(left.X - right.X, left.Y - right.Y); }
    public static bool operator ==(PositionI left, PositionI right) { return left.Equals(right); }
    public static bool operator !=(PositionI left, PositionI right) { return !left.Equals(right); }

    public bool Equals(PositionI other)
    {
      return X == other.X && Y == other.Y;
    }

    //TODO: GPT
    public override int GetHashCode()
    {
      unchecked
      {
        int hash = 17;
        hash = hash * 31 + X;
        hash = hash * 31 + Y;
        return hash;
      }
    }

    public override string ToString()
    {
      return $"({X},{Y})";
    }
  }
}
