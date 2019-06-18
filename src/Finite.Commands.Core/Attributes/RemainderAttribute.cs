using System;

namespace Finite.Commands
{
    /// <summary>
    /// Represents a parameter which consumes the remainder of the input string
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public sealed class RemainderAttribute : Attribute
    { }
}
