using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnDemandTutor.Core.Base;
using OnDemandTutor.Repositories.Entity;

namespace OnDemandTutor.Contract.Repositories.Entity
{
    public class Complaint : BaseEntity
    {
        public Guid StudentId { get; set; }
        public Guid TutorId { get; set; }

        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Status { get; set; }

        // Navigation properties
        public virtual Accounts Accounts { get; set; }
        public virtual Slot Slot { get; set; }


    }
}
