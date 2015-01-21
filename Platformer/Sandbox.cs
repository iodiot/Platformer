using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Platformer
{
  public class Sandbox
  {
    private readonly SpriteBatch spriteBatch;

    private Texture2D oneWhitePixel;

    private readonly List<Actor> actors, actorsToAdd, actorsToRemove;

    private PlayerActor player;

    private ActorMap actorMap;

    private int counter;

    public Sandbox(GraphicsDeviceManager graphics, SpriteBatch spriteBatch)
    {
      this.spriteBatch = spriteBatch;

      actors = new List<Actor>();
      actorsToAdd = new List<Actor>();
      actorsToRemove = new List<Actor>();

      AddActors();
    }

    private void AddActors()
    {
      AddActor(new PlatformActor(this, new Rectangle(50, 200, 300, 50)));

      for (var i = 0; i < 15; ++i)
      {
        AddActor(new PlatformActor(this, new Rectangle(150 + i * 10, 190 - i * 10, 50, 10)));
      }

      AddActor(new PlatformActor(this, new Rectangle(350, 250, 100, 50)));
      AddActor(new PlatformActor(this, new Rectangle(450, 200, 100, 50)));

      AddActor(new PlatformActor(this, new Rectangle(600, 200, 1000, 50)));

      //AddActor(new MovingPlatformActor(this, new Rectangle(700, 150, 100, 25)));

      player = new PlayerActor(this, new Vector2(100, 50));
      AddActor(player);

      AddActor(new EnemyActor(this, new Vector2(1500, 0)));
      //AddActor(new EnemyActor(this, new Vector2(1550, 0)));
      //AddActor(new EnemyActor(this, new Vector2(1600, 0)));
      //AddActor(new EnemyActor(this, new Vector2(1650, 0)));
    }

    public void AddActor(Actor actor)
    {
      actorsToAdd.Add(actor);
    }

    public void RemoveActor(Actor actor)
    {
      actorsToRemove.Add(actor);

      if (actor is EnemyActor)
      {
        AddActor(new EnemyActor(this, new Vector2(1500, 0)));

      }
    }

    public void Load(ContentManager content)
    {
      oneWhitePixel = content.Load<Texture2D>("one");
    }

    public void Update()
    {
      // generate actors map
      actorMap = new ActorMap(0, 2000, 100);
      foreach (var a in actors)
      {
        actorMap.AddActor(a);
      }

      foreach (var a in actors)
      {
        a.Update();
      }

      Step();

      // remove actors
      foreach (var a in actorsToRemove)
      {
        actors.Remove(a);
      }
      actorsToRemove.Clear();

      // add actors
      actors.AddRange(actorsToAdd);
      actorsToAdd.Clear();

      Console.WriteLine(counter);
      counter = 0;
    }
      
    public void Draw()
    {
      foreach (var a in actors)
      {
        // draw bounding box
        var box = a.GetWorldBoundingBox();
        box.X -= (int)player.Position.X - 200;
        box.Y -= (int)(player.Position.Y * 0.25f) - 50;
        DrawRectangle(box, a.BoundingColor * (a.TintTtl > 0 ? 0.5f : 1.0f));

        // draw colliders
        for (var i = 0; i < a.GetCollidersCount(); ++i)
        {
          var collider = a.GetWorldCollider(i);
          collider.BoundingBox.X -= (int)player.Position.X - 200;
          collider.BoundingBox.Y -= (int)(player.Position.Y * 0.25f) - 50;
          DrawRectangle(collider.BoundingBox, Color.White * ((a.Ticks / 25) % 2 == 0 ? 0.5f : 0.25f));
        }
      }
    }

    public void DrawRectangle(Rectangle rect, Color tint)
    {
      spriteBatch.Draw(
        oneWhitePixel,
        new Vector2(rect.X, rect.Y),
        rect,
        tint
      );
    }

    public void Step()
    {
      const float G = 5.0f;
      const float Eps = 0.01f;

      // apply gravity
      foreach (var a in actors)
      {
        if (!a.IsStatic && a.IsGravitable)
        {
          a.Velocity.Y += G;
        }
      }
        
      foreach (var a in actors)
      {
        if (!a.IsStatic)
        {
          ResolveColliders(a);
        }
      }


      foreach (var a in actors)
      {
        if (!a.IsStatic)
        {
          ResolveBoundingBoxes(a);
        }
      }

      foreach (var a in actors)
      {
        if (!a.IsStatic)
        {
          LimitVelocity(a);
        }
      }

      // add velocity
      foreach (var a in actors)
      {
        if (a.IsStatic)
        {
          continue;
        }

        if (a.Velocity.Length() < Eps)
        {
          a.Velocity = Vector2.Zero;
          continue;
        }

        a.Position += a.Velocity;
        a.Velocity = Vector2.Zero;
      }
    }

    private void ResolveBoundingBoxes(Actor actor)
    {
      var obstacles = actorMap.FetchActors(actor.GetWorldBoundingBox());

      foreach (var obstacle in obstacles)
      {
        if (actor.GetWorldBoundingBox().Intersects(obstacle.GetWorldBoundingBox()))
        {
          actor.OnBoundingBoxTrigger(obstacle);
          obstacle.OnBoundingBoxTrigger(actor);
        }
      }
    }

    private void ResolveColliders(Actor actor)
    {
      for (var i = 0; i < actor.GetCollidersCount(); ++i)
      {
        var collider = actor.GetWorldCollider(i);

        var actorsInRadius = actorMap.FetchActors(collider.BoundingBox);

        foreach (var other in actorsInRadius)
        {
          if (actor == other)
          {
            continue;
          }

          for (var j = 0; j < other.GetCollidersCount(); ++j)
          {
            var otherCollider = other.GetWorldCollider(j);

            if (collider.BoundingBox.Intersects(otherCollider.BoundingBox))
            {
              actor.OnColliderTrigger(other, j, i);
            }
          }

          ++counter;
        }
      }
    }

    private void LimitVelocity(Actor actor)
    {
      const float Eps = 0.01f;
      const float LickStep = -10.0f;

      if (actor.Velocity.Length() < Eps)
      {
        return;
      }

      // y-axis
      if (Math.Abs(actor.Velocity.Y) > Eps)
      {
        var obstaclesY = GetObstacles(actor, 0, actor.Velocity.Y);
        if (obstaclesY.Count > 0)
        {
          var minY = (int)Math.Abs(actor.Velocity.Y);
          var box = actor.GetWorldBoundingBox();
          foreach (var o in obstaclesY)
          {
            var otherBox = o.GetWorldBoundingBox();

            var topBox = box.Y < otherBox.Y ? box : otherBox;
            var bottomBox = box.Y < otherBox.Y ? otherBox : box;

            var y = Math.Abs(topBox.Y + topBox.Height - bottomBox.Y);
            if (y < minY)
            {
              minY = y;
            }
          }

          actor.Velocity.Y = minY * Math.Sign(actor.Velocity.Y);
        }
      }

      // x-axis
      if (Math.Abs(actor.Velocity.X) > Eps)
      {
        var obstaclesX = GetObstacles(actor, actor.Velocity.X, 0);
        if (obstaclesX.Count > 0)
        {
          var minX = (int)Math.Abs(actor.Velocity.X);
          var box = actor.GetWorldBoundingBox();
          foreach (var o in obstaclesX)
          {
            var otherBox = o.GetWorldBoundingBox();

            var leftBox = box.X < otherBox.X ? box : otherBox;
            var rightBox = box.X < otherBox.X ? otherBox : box;

            var x = Math.Abs(rightBox.X - leftBox.X - leftBox.Width);
            if (x < minX)
            {
              minX = x;
            }
          }

          // try to lick
          if (minX == 0 && GetObstacles(actor, actor.Velocity.X, LickStep).Count == 0)
          {
            actor.Velocity.Y = LickStep;
            return;
          }

          actor.Velocity.X = minX * Math.Sign(actor.Velocity.X);
        }
      }

      // final check
      if (actor.Velocity.Length() > Eps && GetObstacles(actor, actor.Velocity.X, actor.Velocity.Y).Count > 0)
      {
        Console.WriteLine("NO WAY");

        // try to lick 
        if (GetObstacles(actor, actor.Velocity.X, -1.0f).Count == 0)
        {
          actor.Velocity.Y = -1.0f;
        }
        else if (GetObstacles(actor, actor.Velocity.X, 1.0f).Count == 0)
        {
          actor.Velocity.Y = 1.0f;
        }
        else
        {
          actor.Velocity = Vector2.Zero;
        }
      }
    }

    public List<Actor> GetObstacles(Actor actor, float dx, float dy)
    {
      var result = new List<Actor>();

      var box = actor.GetBoundingBox();

      box.X = (int)(box.X + dx + actor.Position.X);
      box.Y = (int)(box.Y + dy + actor.Position.Y);

       var actorsInRadius = actorMap.FetchActors(box);

      foreach (var other in actorsInRadius)
      {
        if (actor == other || other.IsPassableFor(actor))
        {
          continue;
        }

        if (box.Intersects(other.GetWorldBoundingBox()))
        {
          result.Add(other); 
          other.TintTtl = 5;
        }
      }

      return result;
    }
  }
}

