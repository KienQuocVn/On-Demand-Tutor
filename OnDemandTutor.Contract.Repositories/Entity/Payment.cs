using OnDemandTutor.Core.Base;
using OnDemandTutor.Repositories.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnDemandTutor.Contract.Repositories.Entity
{
    public class Payment : BaseEntity
    {
        public string BankAccountNumber { get; set; }
        public string Bank { get; set; }

        // Navigation properties
        public virtual Accounts Accounts { get; set; }
    }
}
