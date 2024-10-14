using OnDemandTutor.Core.Base;
using OnDemandTutor.Repositories.Entity;

namespace OnDemandTutor.Contract.Repositories.Entity
{
    public class Class : BaseEntity
    {
        public Guid AccountId { get; set; }
        public Guid SubjectId { get; set; }
        public int AmountOfSlot { get; set; }
        public DateTime StartDay { get; set; }
        public DateTime EndDay { get; set; }

        // Navigation properties
        public virtual Accounts account { get; set; }
        public virtual Subject Subject { get; set; }
        public virtual ICollection<Slot> Slots { get; set; }

        public virtual ICollection<Feedback> Feedbacks { get; set; }
        public virtual ICollection<Complaint> Complaints { get; set; }
    }
}
