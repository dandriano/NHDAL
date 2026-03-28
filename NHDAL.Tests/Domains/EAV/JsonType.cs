using NHibernate;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Data;
using System.Data.Common;
using System.Text.Json;

namespace NHDAL.Tests.Domains.EAV
{
    public class JsonType<T> : IUserType where T : class
    {
        public virtual SqlType[] SqlTypes => new[] {
            new NpgsqlType(DbType.Object, NpgsqlDbType.Json)
        };

        public Type ReturnedType => typeof(T);

        public bool IsMutable => true;

        public object Assemble(object cached, object owner)
        {
            return cached;
        }

        public object? DeepCopy(object? value)
        {
            if (value is not T source)
                return null;

            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(source)); ;
        }

        public object Disassemble(object value)
        {
            return value;
        }

        public new bool Equals(object? x, object? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;

            return JsonSerializer.Serialize(x) == JsonSerializer.Serialize(y);
        }

        public int GetHashCode(object? x)
        {
            if (x is null)
                return 0;

            return JsonSerializer.Serialize(x).GetHashCode();
        }

        public object? NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
        {
            var value = NHibernateUtil.String.NullSafeGet(rs, names[0], session, owner) as string;
            if (!string.IsNullOrEmpty(value))
                return JsonSerializer.Deserialize<T>(value);

            return null;
        }

        public virtual void NullSafeSet(DbCommand cmd, object? value, int index, ISessionImplementor session)
        {
            NHibernateUtil.String.NullSafeSet(cmd, JsonSerializer.Serialize(value as T), index, session);
        }

        public object Replace(object original, object target, object owner)
        {
            return original;
        }
    }

    public class JsonbType<T> : JsonType<T> where T : class
    {
        public override SqlType[] SqlTypes => new SqlType[] {
            new NpgsqlType(DbType.Binary, NpgsqlDbType.Jsonb)
        };

        public override void NullSafeSet(DbCommand cmd, object? value, int index, ISessionImplementor session)
        {
            var parameter = (NpgsqlParameter)cmd.Parameters[index];

            if (value == null)
            {
                parameter.Value = DBNull.Value;
            }
            else if (SqlTypes[0] is NpgsqlType type)
            {
                parameter.NpgsqlDbType = type.NpgDbType;
                parameter.Value = JsonSerializer.Serialize(value);
            }
        }
    }
}
