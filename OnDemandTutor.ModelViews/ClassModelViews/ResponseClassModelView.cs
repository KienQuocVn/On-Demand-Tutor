using System;
using System.Collections.Generic;
using System.Linq;


namespace OnDemandTutor.ModelViews.ClassModelViews
{
    public class ResponseClassModelView
    {
        public Guid Id { get; set; }
        public Guid TutorId { get; set; }
        public Guid SubjectId { get; set; }
        public int AmountOfSlot { get; set; }
        public DateTime StartDay { get; set; }
        public DateTime EndDay { get; set; }

        public string? CreatedBy { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public string? DeletedBy { get; set; }
        public DateTimeOffset? DeletedTime { get; set; }
        public string? LastUpdatedBy { get; set; }
        public DateTimeOffset LastUpdateTime { get; set; }
    }
}
