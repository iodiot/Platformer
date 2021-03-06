﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Platformer
{
  public class Actor
  {
    protected readonly Sandbox sandbox;

    public Vector2 Position, Velocity;
    public Color BoundingColor;
    public bool IsStatic;
    public bool IsGravitable;
    public int TintTtl;
    public int Ticks;

    protected readonly List<Collider> colliders;
    protected Rectangle boundingBox;

    public Actor(Sandbox sandbox, Vector2 position)
    {
      this.sandbox = sandbox;

      Position = position;

      colliders = new List<Collider>();
    }

    public virtual void Update()
    {
      if (TintTtl > 0)
      {
        --TintTtl;
      }

      ++Ticks;
    }

    public Rectangle GetWorldBoundingBox()
    {
      return new Rectangle(
        (int)(Position.X + boundingBox.X),
        (int)(Position.Y + boundingBox.Y),
        boundingBox.Width,
        boundingBox.Height
      );
    }

    public Rectangle GetBoundingBox()
    {
      return boundingBox;
    }

    public int GetCollidersCount()
    {
      return colliders.Count;
    }

    public Collider GetWorldCollider(int n)
    {
      var box = colliders[n].BoundingBox;

      var collider = new Collider()
      {
          Name = colliders[n].Name,
          Color = colliders[n].Color,
          BoundingBox = new Rectangle((int)(Position.X + box.X), (int)(Position.Y + box.Y), box.Width, box.Height)
      };

      return collider;
    }

    public Collider GetCollider(int n)
    {
      return colliders[n];
    }

    public virtual bool IsPassableFor(Actor actor)
    {
      return false;
    }

    public virtual void OnColliderTrigger(Actor other, int otherCollider, int thisCollider)
    {
    }

    public virtual void OnBoundingBoxTrigger(Actor other)
    {
    }
  }
}

