using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Platformer
{
  public class PlayerActor : Actor
  {
    private int fireTtl;

    public PlayerActor(Sandbox sandbox, Vector2 position) : base(sandbox, position)
    {
      IsStatic = false;
      IsGravitable = true;
      BoundingColor = Color.Yellow;
      boundingBox = new Rectangle(0, 0, 25, 50);

      colliders.Add(new Collider() { BoundingBox = new Rectangle(0, 0, 25, 25) });
    }

    public override void Update()
    {
      HandleInput();

      if (fireTtl > 0)
      {
        --fireTtl;
      }
     
      base.Update();
    }

    public void HandleInput()
    {
      const float Step = 5.0f;

      //Velocity.X += step;

      var keyboardState = Keyboard.GetState();

      if (keyboardState.IsKeyDown(Keys.Up))
      {
        Velocity.Y -= Step * 2.0f;
      }

      if (keyboardState.IsKeyDown(Keys.Left))
      {
        Velocity.X -= Step;
      }

      if (keyboardState.IsKeyDown(Keys.Right))
      {
        Velocity.X += Step;
      }

      if (keyboardState.IsKeyDown(Keys.Space) && fireTtl == 0)
      {
        sandbox.AddActor(new ProjectileActor(sandbox, new Vector2(Position.X + 25f, Position.Y)));
        fireTtl = 25;
      }
    }

    public override void OnBoundingBoxTrigger(Actor actor)
    {

      base.OnBoundingBoxTrigger(actor);
    }

    public override bool IsPassableFor(Actor actor)
    {
      return !actor.IsStatic;
    }

    public override void OnColliderTrigger(Actor other, int otherCollider, int thisCollider)
    {
      if (other is EnemyActor)
      {
        Console.WriteLine(String.Format("{0} : hurt", Ticks));
      }

      base.OnColliderTrigger(other, otherCollider, thisCollider);
    }
  }
}

