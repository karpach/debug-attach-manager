using System;

namespace Karpach.DebugAttachManager.Helpers
{
    internal static class BooleanBoxes
    {
        public static readonly Object TrueBox = true;
        public static readonly Object FalseBox = false;

        public static object Box(bool value)
        {
            if (value)
            {
                return TrueBox;
            }
            return FalseBox;
        }
    }
}