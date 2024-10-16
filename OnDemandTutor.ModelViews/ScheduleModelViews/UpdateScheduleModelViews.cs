using System.ComponentModel.DataAnnotations;

namespace OnDemandTutor.ModelViews.ScheduleModelViews
{
    public class UpdateScheduleModelViews
    {

        public string Status { get; set; } 

        public Guid StudentId { get; set; } 
        public Guid SlotId { get; set; } 
    }
}
