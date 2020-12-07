namespace Kizar.MediatR.Caching
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class InvalidateAttribute : Attribute {
        public Type[] Types { get; set; }
    }
}
