using NHibernate.Type;
using System;
using System.Collections.Generic;


namespace NHDAL.Tests.Misc
{
    public class CustomColumnType : CustomType
    {
        private readonly string _fqName;

        public override string Name => _fqName;

        public CustomColumnType(Type userTypeClass, IDictionary<string, string>? parameters) : base(userTypeClass, parameters)
        {
            _fqName = userTypeClass.AssemblyQualifiedName!;
        }
    }
}
