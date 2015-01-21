using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Platformer
{
  public class ActorMap
  {
    private readonly List<Actor>[] blocks;
    private readonly int leftBoundary, rightBoundary, blocksNumber, blockWidth;

    public ActorMap(int leftBoundary, int rightBoundary, int blocksNumber)
    {
      blocks = new List<Actor>[blocksNumber];

      this.leftBoundary = leftBoundary;
      this.rightBoundary = rightBoundary;
      this.blocksNumber = blocksNumber;

      blockWidth = (rightBoundary - leftBoundary) / blocksNumber;
    }

    public void AddActor(Actor actor)
    {
      var startN = GetBlockNumber((int)actor.Position.X);
      var lastN = GetBlockNumber((int)actor.Position.X + actor.GetBoundingBox().Width);

      Debug.Assert(startN >= 0 && lastN < blocksNumber && startN <= lastN, "ActorMap.AddActor() : ");

      for (var n = startN; n <= Math.Min(lastN, blocksNumber - 1); ++n)
      {
        if (blocks[n] == null)
        {
          blocks[n] = new List<Actor>();
        }

        blocks[n].Add(actor);
      }
    }

    private int GetBlockNumber(int x)
    {
      return (x - leftBoundary) / blockWidth;
    }

    public List<Actor> FetchActors(Rectangle rect)
    {
      var startN = GetBlockNumber(rect.X);
      var lastN = GetBlockNumber(rect.X + rect.Width);

      Debug.Assert(startN >= 0 && lastN < blocksNumber && startN <= lastN, "ActorMap.FetchActors() : ");

      var result = new List<Actor>();

      for (var n = startN; n <= Math.Min(lastN, blocksNumber - 1); ++n)
      {
        if (blocks[n] == null)
        {
          continue;
        }

        foreach (var actor in blocks[n])
        {
          if (!result.Contains(actor))
          {
            result.Add(actor);
          }
        }
      }

      return result;
    }
  }
}

