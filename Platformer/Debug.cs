using System;

namespace Platformer
{
  public class Debug
  {
    public static void Assert(bool condition, string message)
    {
      #if DEBUG
      if (!condition)
      {
         throw new Exception(message);
      }
      #endif
    }
  }
}

