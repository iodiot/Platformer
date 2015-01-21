using System;
using Microsoft.Xna.Framework;

namespace Platformer
{
  public class EnemyActor : Actor
  {
    public EnemyActor(Sandbox sandbox, Vector2 position) : base(sandbox, position)
    {
      boundingBox = new Rectangle(0, 0, 50, 150);
      BoundingColor = Color.Green;
      IsStatic = false;
      IsGravitable = true;

      colliders.Add(new Collider() { BoundingBox = new Rectangle(0, 0, 50, 50) });
    }

    public override void Update()
    {
      if (Ticks < 10000)
      {
        Velocity.X -= 1.0f;
      }

     // colliders[0].BoundingBox.X = (int)(Math.Sin(Ticks * 0.1f) * 50.0f);
      colliders[0].BoundingBox.Y = (int)(Math.Cos(Ticks * 0.1f) * 50.0f) + 50;

      base.Update();
    }

    public override bool IsPassableFor(Actor actor)
    {
      return !actor.IsStatic;
    }

    public override void OnColliderTrigger(Actor other, int otherCollider, int thisCollider)
    {
      if (other is ProjectileActor)
      {
        sandbox.RemoveActor(this);
        sandbox.RemoveActor(other);
      }

      base.OnColliderTrigger(other, otherCollider, thisCollider);
    }
  }
}

