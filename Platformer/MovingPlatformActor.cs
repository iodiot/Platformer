using System;
using Microsoft.Xna.Framework;

namespace Platformer
{
  public class MovingPlatformActor : Actor
  {
    private readonly float startX, endX; 
    private int dir;

    public MovingPlatformActor(Sandbox sandbox, Rectangle rect) : base(sandbox, new Vector2(rect.X, rect.Y))
    {
      IsStatic = false;
      IsGravitable = false;
      BoundingColor = Color.Red;
      boundingBox = new Rectangle(0, 0, rect.Width, rect.Height);

      startX = Position.X;
      endX = Position.X + boundingBox.Width + 200.0f;
      dir = 1;
    }

    public override void Update()
    {
      if (Position.X < startX || Position.X >= endX)
      {
        dir *= -1;
      }
 
      Velocity.X += dir * 3.0f;


      base.Update();
    }
  }
}


