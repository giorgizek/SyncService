﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Sync.Model
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class CRMEntities : DbContext
    {
        public CRMEntities()
            : base("name=CRMEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<T_SyncINS> T_SyncINS { get; set; }
        public virtual DbSet<ClinicPerformedService> ClinicPerformedServices { get; set; }
    
        public virtual ObjectResult<T_SyncINS> SP_T_SyncINS_Get()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<T_SyncINS>("SP_T_SyncINS_Get");
        }
    
        public virtual ObjectResult<T_SyncINS> SP_T_SyncINS_Get(MergeOption mergeOption)
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<T_SyncINS>("SP_T_SyncINS_Get", mergeOption);
        }
    }
}
