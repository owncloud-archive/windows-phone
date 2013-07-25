namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Enables automaticly resolving of calling member
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    public sealed class CallerMemberNameAttribute : Attribute { }
}