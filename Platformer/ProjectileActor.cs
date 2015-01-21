using System;
using Microsoft.Xna.Framework;

namespace Platformer
{
  public class ProjectileActor : Actor
  {
    public ProjectileActor(Sandbox sandbox, Vector2 position) : base(sandbox, position)
    {
      boundingBox = new Rectangle(0, 0, 25, 25);
      BoundingColor = Color.Yellow ;
      IsStatic = false;
      IsGravitable = false;

      colliders.Add(new Collider(){ BoundingBox = boundingBox });
    }

    public override void Update()
    {
      Velocity.X += 15.0f;

      base.Update();
    }

    public override void OnBoundingBoxTrigger(Actor other)
    {
      if (other is PlatformActor)
      {
        sandbox.RemoveActor(this);
      }

      base.OnBoundingBoxTrigger(other);
    }
  }
}

