using NHibernate;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Text.Json;

namespace NHDAL.Tests.Misc
{
    [Serializable]
    public class JsonType<TSerializable> : IUserType where TSerializable : class
    {
        private readonly Type _serializableClass = typeof(TSerializable);

        public SqlType[] SqlTypes => [new NpgsqlExtendedSqlType(DbType.Object, NpgsqlDbType.Jsonb)];
        public Type ReturnedType => _serializableClass;
        public bool IsMutable => true;

        public new bool Equals(object x, object y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (x == null || y == null)
                return false;

            if (IsDictionary(x) && IsDictionary(y))
                return EqualDictionary(x, y);

            return x.Equals(y);
        }

        private static bool EqualDictionary(object x, object y)
        {
            var a = x as IDictionary;
            var b = y as IDictionary;

            if (a!.Count != b!.Count) return false;

            foreach (var key in a.Keys)
            {
                if (!b.Contains(key)) return false;

                var va = a[key];
                var vb = b[key];

                if (!va!.Equals(vb)) return false;
            }

            return true;
        }

        private static bool IsDictionary(object o)
        {
            return typeof(IDictionary).IsAssignableFrom(o.GetType());
        }

        public int GetHashCode(object x)
        {
            return x.GetHashCode();
        }

        public object? NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
        {
            var value = NHibernateUtil.String.NullSafeGet(rs, names[0], session, owner) as string;
            if (!string.IsNullOrEmpty(value))
                return JsonSerializer.Deserialize<TSerializable>(value);

            return null;
        }

        public void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
        {
            var parameter = cmd.Parameters[index];

            if (parameter is NpgsqlParameter)
                parameter.DbType = SqlTypes[0].DbType;

            if (value == null)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = value;
        }

        public object? DeepCopy(object value)
        {
            return JsonSerializer.Deserialize<TSerializable>(JsonSerializer.Serialize(value));
        }

        public object Replace(object original, object target, object owner)
        {
            return original;
        }

        public object? Assemble(object cached, object owner)
        {
            if (cached != null)
                return JsonSerializer.Deserialize<TSerializable>((string)cached);

            return null;
        }

        public object? Disassemble(object value)
        {
            if (value != null)
                return JsonSerializer.Serialize(value);

            return null;
        }
    }
}
