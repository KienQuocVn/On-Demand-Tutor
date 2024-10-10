
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

        public string SlotId { get; set; }
        public string ClassId { get; set; }
        public string FeedbackText { get; set; }
    }
}