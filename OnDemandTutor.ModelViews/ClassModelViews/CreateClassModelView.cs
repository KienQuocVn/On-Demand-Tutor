

namespace OnDemandTutor.ModelViews.ClassModelViews
{
    public class CreateClassModelView
    {
        public Guid AccountId { get; set; }
        public Guid SubjectId { get; set; }
        public int AmountOfSlot { get; set; }
        public DateTime StartDay { get; set; }
        public DateTime EndDay { get; set; }
    }
}
