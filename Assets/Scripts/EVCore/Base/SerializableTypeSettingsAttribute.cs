using System;
using System.Diagnostics;

namespace EverlastingEditor.Base
{
    public enum SerializableTypeCollection
    {
        All,
        Primitive,
        Serializable,
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    [Conditional("UNITY_EDITOR")]
    public class SerializableTypeSettingsAttribute : Attribute
    {
        public SerializableTypeCollection Collection { get; }

        public SerializableTypeSettingsAttribute(SerializableTypeCollection collection)
        {
            Collection = collection;
        }
    }
}