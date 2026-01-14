using Godot;
using System;

namespace RealTimeSurvivors.Components.Shared
{
  /// <summary>
  /// Floating point representation of a Position.
  /// Used for representing a entity's position in World Space.
  /// </summary>
  public struct Position
  {
    public (float x, float y) Value;
    public float X => Value.x;
    public float Y => Value.y;

    public Position(float x, float y)
    {
      Value = (x, y);
    }

    public float DistanceTo(Position to)
    {
      return (float)Math.Sqrt((X - to.X) * (X - to.X) + (Y - to.Y) * (Y - to.Y));
    }

    public float DistanceToRect(Rect2 rect)
    {
      float dx = 0f;
      if (X < rect.Position.x)
        dx = rect.Position.x - X;
      else if (X > rect.Position.x + rect.Size.x)
        dx = X - (rect.Position.x + rect.Size.x);

      float dy = 0f;
      if (Y < rect.Position.y)
        dy = rect.Position.y - Y;
      else if (Y > rect.Position.y + rect.Size.y)
        dy = Y - (rect.Position.y + rect.Size.y);

      return Mathf.Sqrt(dx * dx + dy * dy);
    }

    public static explicit operator PositionI(Position p) { return new PositionI((int)p.X, (int)p.Y); }
    public static explicit operator Vector2(Position p) { return new Godot.Vector2(p.X, p.Y); }

    public static Position operator +(Position left, Position right) { return new Position(left.X + right.X, left.Y + right.Y); }
    public static Position operator -(Position left, Position right) { return new Position(left.X - right.X, left.Y - right.Y); }
    public static Position operator *(Position left, Position right) { return new Position(left.X * right.X, left.Y * right.Y); }
    public static Position operator *(Position left, int right) { return new Position(left.X * right, left.Y * right); }
    public static Position operator *(Position left, float right) { return new Position(left.X * right, left.Y * right); }
    public static bool operator ==(Position left, Position right) { return left.Equals(right); }
    public static bool operator !=(Position left, Position right) { return !left.Equals(right); }

    public bool Equals(Position other)
    {
      return X == other.X && Y == other.Y;
    }

    //TODO: GPT
    public override int GetHashCode()
    {
      unchecked
      {
        int hash = 17;
        hash = hash * 31 + (int)X;
        hash = hash * 31 + (int)Y;
        return hash;
      }
    }

    public override string ToString()
    {
      return $"({X},{Y})";
    }
  }
}
