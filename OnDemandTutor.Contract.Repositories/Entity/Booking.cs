using System;
using OnDemandTutor.Core.Base;
using OnDemandTutor.Repositories.Entity;

namespace OnDemandTutor.Contract.Repositories.Entity
{
    public class Booking : BaseEntity
    {
        //public string Id { get; set; } = Guid.NewGuid().ToString(); // thay đổi
        public Guid StudentId { get; set; }
        public virtual Accounts Student { get; set; }

        public string SubjectId { get; set; }
        public virtual Subject Subject { get; set; }

        public DateTime BookingDate { get; set; }
        public string TutorSubjectId { get; set; }
        public virtual TutorSubject TutorSubject { get; set; }
    }
}
