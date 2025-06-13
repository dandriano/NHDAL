using NHDAL.Tests.Domains.EAV.Entities;
using System;

namespace NHDAL.Tests.Domains.EAV
{
    public class ConnectionRecord : EntityRecord
    {
        public float Weight
        {
            get
            {
                var val = GetAttributeValue("weight");
                return float.TryParse(val, out var w) ? w : 0f;
            }
        }

        public Guid SourceVertexId
        {
            get
            {
                var val = GetAttributeValue("sourceId");
                return Guid.TryParse(val, out var id) ? id : Guid.Empty;
            }
        }

        public Guid TargetVertexId
        {
            get
            {
                var val = GetAttributeValue("targetId");
                return Guid.TryParse(val, out var id) ? id : Guid.Empty;
            }
        }
    }
}
