// Guids.cs
// MUST match guids.h
using System;

namespace comScoreInc.DebugAttachManager
{
    static class GuidList
    {
        public const string guidDebugAttachManagerPkgString = "facf6f74-1cc6-44be-b57d-115d48b28821";
        public const string guidDebugAttachManagerCmdSetString = "66990a61-9e85-432c-aa61-417a20bffe94";
        public const string guidToolWindowPersistanceString = "422cb21c-c839-43b4-98fe-5c2b1ef87fc6";

        public static readonly Guid guidDebugAttachManagerCmdSet = new Guid(guidDebugAttachManagerCmdSetString);
    };
}