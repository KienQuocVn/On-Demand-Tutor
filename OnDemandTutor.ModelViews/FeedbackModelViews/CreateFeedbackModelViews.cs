
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnDemandTutor.ModelViews.FeedbackModelViews
{
    public class CreateFeedbackModelViews
    {
        public Guid StudentId { get; set; }
        public Guid TutorId { get; set; }

        public Guid SlotId { get; set; }
        public Guid ClassId { get; set; }
        public string FeedbackText { get; set; }
    }
}