using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sync.Model
{
    public enum DD_InsMedicalProviderInvoiceStatus
    {
        Passive = 1,
        Active = 2,
        Complete = 3,
        Completewithcorrection = 4,
        Completeapproved = 5,
        Completeapprovedwithcorrection = 6,
        Act = 7,
        Actwithcorrection = 8,
        Closed = 9,
        Closedwithcorrection = 10,
    }
}
