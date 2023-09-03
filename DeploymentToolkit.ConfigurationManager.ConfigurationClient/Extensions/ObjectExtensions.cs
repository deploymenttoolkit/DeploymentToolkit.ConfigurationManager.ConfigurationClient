using System;
using System.Collections.ObjectModel;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Extensions
{
    public static class ObjectExtensions
    {
        public static bool IsNumericType(this Type type)
        {
            var typeCode = Type.GetTypeCode(type);
            switch (typeCode)
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsObservableCollection(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ObservableCollection<>);
        }
    }
}
