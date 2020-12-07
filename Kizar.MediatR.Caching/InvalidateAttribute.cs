namespace Kizar.MediatR.Caching {
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class InvalidateAttribute : Attribute {
        public InvalidateAttribute(params Type[] types) {
            this.Types = types;
        }
        public Type[] Types { get; }
    }
}
