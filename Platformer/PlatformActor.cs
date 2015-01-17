using System;
using Microsoft.Xna.Framework;

namespace Platformer
{
  public class PlatformActor : Actor
  {
    public PlatformActor(Sandbox sandbox, Rectangle rect) : base(sandbox, new Vector2(rect.X, rect.Y))
    {
      IsStatic = true;
      IsGravitable = false;
      BoundingColor = Color.Red;
      boundingBox = new Rectangle(0, 0, rect.Width, rect.Height);
    }
  }
}

