using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Infrastructure;

namespace Sync.Model
{
    public partial class GPIHEntities
    {
        public static GPIHEntities CreateInstance(string connectionString)
        {
            var entityBuilder = new EntityConnectionStringBuilder
            {
                Provider = "System.Data.SqlClient",
                ProviderConnectionString = connectionString,
                Metadata = @"res://*/GPIH.csdl|res://*/GPIH.ssdl|res://*/GPIH.msl"
            };

            return new GPIHEntities(entityBuilder.ToString());
        }

         public GPIHEntities(string connectionString, int? commandTimeOut = null)
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
