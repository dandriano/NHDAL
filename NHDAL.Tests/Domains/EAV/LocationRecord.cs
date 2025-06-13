using NHDAL.Tests.Domains.EAV.Entities;

namespace NHDAL.Tests.Domains.EAV
{
    public class LocationRecord : EntityRecord
    {
        public string Name
        {
            get => GetAttributeValue("name") ?? string.Empty;
        }
    }
}
