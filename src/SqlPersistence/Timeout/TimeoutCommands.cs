﻿#pragma warning disable 1591
namespace NServiceBus.Persistence.Sql
{
    using System;

    /// <summary>
    /// Not for public use.
    /// </summary>
    [Obsolete("Not for public use")]
    public class TimeoutCommands
    {
        public TimeoutCommands(string removeById, string next, string peek, string add, string removeBySagaId, string range)
        {
            RemoveById = removeById;
            Next = next;
            Peek = peek;
            Add = add;
            RemoveBySagaId = removeBySagaId;
            Range = range;
        }
        public string RemoveById { get; }
        public string Next { get; }
        public string Peek { get; }
        public string Add { get; }
        public string RemoveBySagaId { get; }
        public string Range { get; }
    }
}