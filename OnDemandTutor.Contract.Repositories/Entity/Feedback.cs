using OnDemandTutor.Core.Base;
using OnDemandTutor.Repositories.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnDemandTutor.Contract.Repositories.Entity
{
    public class Feedback : BaseEntity
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public Guid TutorId { get; set; }
        public string FeedbackText { get; set; }
        public Guid SlotId { get; set; }
        public Guid ClassId { get; set; }
        public int NumberOfViolations { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Accounts Accounts { get; set; }
        public virtual Slot Slot { get; set; }
        public virtual Class Class { get; set; }
    }
}
