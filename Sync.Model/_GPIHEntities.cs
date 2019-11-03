using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Infrastructure;

namespace Sync.Model
{
    public partial class INSEntities
    {
        public static INSEntities CreateInstance(string connectionString)
        {
            var entityBuilder = new EntityConnectionStringBuilder
            {
                Provider = "System.Data.SqlClient",
                ProviderConnectionString = connectionString,
                Metadata = @"res://*/INS.csdl|res://*/INS.ssdl|res://*/INS.msl"
            };

            return new INSEntities(entityBuilder.ToString());
        }

         public INSEntities(string connectionString, int? commandTimeOut = null)
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
