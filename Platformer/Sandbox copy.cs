using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Platformer
{
  public class Sandbox
  {
    private readonly GraphicsDeviceManager graphics;
    private readonly SpriteBatch spriteBatch;

    private readonly int screenWidth;
    private readonly int screenHeight;

    private Texture2D oneWhitePixel;

    private readonly List<Actor> actors, actorsToAdd, actorsToRemove;

    private PlayerActor player;
    private int counter, maxCounter;

    private ActorMap actorMap;

    public Sandbox(GraphicsDeviceManager graphics, SpriteBatch spriteBatch)
    {
      this.graphics = graphics;
      this.spriteBatch = spriteBatch;

      actors = new List<Actor>();
      actorsToAdd = new List<Actor>();
      actorsToRemove = new List<Actor>();

      screenWidth = graphics.GraphicsDevice.Viewport.Width;
      screenHeight = graphics.GraphicsDevice.Viewport.Height;

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

      AddActor(new MovingPlatformActor(this, new Rectangle(700, 150, 100, 25)));

      player = new PlayerActor(this, new Vector2(100, 50));
      AddActor(player);

      //AddActor(new EnemyActor(this, new Vector2(1500, 0)));
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
      counter = 0;

      // generate actors map
      actorMap = new ActorMap(0, 2000, 100);
      foreach (var a in actors)
      {
    //    actorMap.AddActor(a);
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

      if (maxCounter < counter)
      {
        maxCounter = counter;
        Console.WriteLine(maxCounter);
      }
      counter = 0;
    }
      
    public void Draw()
    {
      foreach (var a in actors)
      {
        // draw bounding box
        var box = a.GetBoundingBox();
        box.X -= (int)player.Position.X - 200;
        box.Y -= (int)(player.Position.Y * 0.25f) - 50;
        DrawRectangle(box, a.BoundingColor * (a.TintTtl > 0 ? 0.5f : 1.0f));

        // draw colliders
        for (var i = 0; i < a.GetCollidersCount(); ++i)
        {
          var collider = a.GetCollider(i);
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

      // add gravity
      foreach (var a in actors)
      {
        if (!a.IsStatic && a.IsGravitable)
        {
          a.Velocity.Y += G;
        }
      }

    
      
      // resolve colliders
      foreach (var a in actors)
      {
        if (!a.IsStatic)
        {
          ResolveColliders(a);
        }
      }

      // resolve collisions
      foreach (var a in actors)
      {
        if (!a.IsStatic && !(a is MovingPlatformActor))
        {
          ResolveCollisions(a);
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

    private void ResolveColliders(Actor actor)
    {
      for (var i = 0; i < actor.GetCollidersCount(); ++i)
      {
        var collider = actor.GetCollider(i);

        foreach (var other in actors)
        {
          if (actor == other)
          {
            continue;
          }

          for (var j = 0; j < other.GetCollidersCount(); ++j)
          {
            var otherCollider = other.GetCollider(j);

            if (collider.BoundingBox.Intersects(otherCollider.BoundingBox))
            {
              actor.OnCollider(other, j, i);
            }
          }
        }
      }
    }

    private void ResolveCollisions(Actor actor)
    {
      const int LickStep = 10;

      var vx = (int)actor.Velocity.X;
      var vy = (int)actor.Velocity.Y;

      // notify actor if collisions will take place 
      var collisions = TryCollide(actor, new Vector2(vx, vy));
      foreach (var c in collisions)
      {
        actor.OnCollide(c);

        if (c is MovingPlatformActor)
        {
          actor.Velocity.X = c.Velocity.X;
        }
      }

      // x axis
      if (Math.Abs(vx) > 0)
      {
        var dx = Math.Sign(vx);
        var x = 0;

        while (x != vx)
        {
          if (TryCollide(actor, new Vector2(x + dx, 0)).Count > 0)
          {
            break;
          }

          x += dx;
        }

        // try to lick
        if (x == 0 && TryCollide(actor, new Vector2(vx, -LickStep)).Count == 0)
        {
          actor.Velocity.Y = -LickStep;
          return;
        }

        actor.Velocity.X = x;
      }

      // y axis
      if (Math.Abs(vy) > 0)
      {
        var dy = Math.Sign(vy);
        var y = 0;

        while (y != vy)
        {
          if (TryCollide(actor, new Vector2(0, y + dy)).Count > 0)
          {
            break;
          }

          y += dy;
        }

        actor.Velocity.Y = y;
      }

      if (TryCollide(actor, actor.Velocity).Count > 0)
      {
        // try to lick
        if (TryCollide(actor, new Vector2(actor.Velocity.X, -LickStep)).Count == 0)
        {
          actor.Velocity.Y = -LickStep;
        }
        else
        {
          actor.Velocity = Vector2.Zero;
    
        }
      }
    }

    public List<Actor> TryCollide(Actor actor, Vector2 v)
    {
      var result = new List<Actor>();

      var box = actor.GetBoundingBox();

      box.X = (int)(box.X + v.X);
      box.Y = (int)(box.Y + v.Y);

     // var actorsInRadius = actorMap.FetchActors(box);

      foreach (var other in actors)
      {
        if (actor == other || other.IsPassableFor(actor))
        {
          continue;
        }

        if (box.Intersects(other.GetBoundingBox()))
        {
          result.Add(other); 
          other.TintTtl = 5;
        }

        ++counter;
      }

      return result;
    }
  }
}

