﻿

namespace OnDemandTutor.ModelViews.ClassModelViews
{
    public class UpdateClassModelView
    {
        public Guid AccountId { get; set; }
        public string SubjectId { get; set; }
        public int AmountOfSlot { get; set; }
        public DateTime StartDay { get; set; }
        public DateTime EndDay { get; set; }
    }
}
