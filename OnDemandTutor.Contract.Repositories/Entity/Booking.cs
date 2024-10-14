using System;
using OnDemandTutor.Core.Base;
using OnDemandTutor.Repositories.Entity;

namespace OnDemandTutor.Contract.Repositories.Entity
{
    public class Booking : BaseEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public Guid StudentId { get; set; }
        public virtual Accounts Student { get; set; }

        public string SubjectId { get; set; }
        public virtual Subject Subject { get; set; }

        public DateTime BookingDate { get; set; }
        public Guid TutorSubjectId { get; set; }
        public virtual TutorSubject TutorSubject { get; set; }
    }
}
