// Simple helper class that allows you to serialize System.Type objects.
// Use it however you like, but crediting or even just contacting the author would be appreciated (Always
// nice to see people using your stuff!)
//
// Written by Bryan Keiren (http://www.bryankeiren.com)

using UnityEngine;
using System.Runtime.Serialization;

namespace Everlasting.Base
{
    [System.Serializable]
    public struct SerializableType
    {
        [SerializeField]
        private string m_AssemblyQualifiedName;

        private System.Type m_SystemType;

        public System.Type SystemType
        {
            get
            {
                if (m_SystemType == null && !string.IsNullOrEmpty(m_AssemblyQualifiedName))
                {
                    m_SystemType = System.Type.GetType(m_AssemblyQualifiedName);
                }

                return m_SystemType;
            }

            set
            {
                m_SystemType = value;
                m_AssemblyQualifiedName = m_SystemType?.AssemblyQualifiedName ?? "";
            }
        }

        public SerializableType(System.Type _SystemType)
        {
            m_SystemType = _SystemType;
            m_AssemblyQualifiedName = _SystemType.AssemblyQualifiedName;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SerializableType))
                return false;

            return this.Equals((SerializableType) obj);
        }

        public bool Equals(SerializableType _Object)
        {
            return _Object.SystemType == SystemType;
        }

        public static bool operator ==(SerializableType a, SerializableType b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(SerializableType a, SerializableType b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return SystemType?.GetHashCode() ?? 0;
        }
    }
}