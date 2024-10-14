using System;
using System.ComponentModel.DataAnnotations;

namespace OnDemandTutor.ModelViews.ScheduleModelViews
{
    public class CreateScheduleModelViews
    {
     
        public Guid StudentId { get; set; }
        public string Status {  get; set; }
        public Guid SlotId { get; set; }


    }
}
