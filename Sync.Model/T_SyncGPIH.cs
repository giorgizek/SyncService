//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class T_SyncINS
    {
        public int ID { get; set; }
        public int ServiceID { get; set; }
        public Nullable<int> INSID { get; set; }
        public byte StatusID { get; set; }
        public int TryCount { get; set; }
        public string ErrorText { get; set; }
        public System.DateTime CreateDate { get; set; }
        public Nullable<System.DateTime> SyncDate { get; set; }
    }
}
