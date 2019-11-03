using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Infrastructure;

namespace Sync.Model
{
    public partial class CRMEntities
    {
        public static CRMEntities CreateInstance(string connectionString)
        {
            var entityBuilder = new EntityConnectionStringBuilder
            {
                Provider = "System.Data.SqlClient",
                ProviderConnectionString = connectionString,
                Metadata = @"res://*/CRM.csdl|res://*/CRM.ssdl|res://*/CRM.msl"
            };

            return new CRMEntities(entityBuilder.ToString());
        }

        public CRMEntities(string connectionString, int? commandTimeOut = null)
            : base(connectionString)
        {
            if (commandTimeOut != null)
            {
                var objectContext = (this as IObjectContextAdapter).ObjectContext;
                objectContext.CommandTimeout = commandTimeOut;
            }
        }
    }
}
