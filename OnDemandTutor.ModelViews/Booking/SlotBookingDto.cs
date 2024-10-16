
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnDemandTutor.ModelViews.Booking
{
    public class SlotBookingDto
    {
        public Guid StudentId { get; set; }
        public Guid SubjectId { get; set; }
        public Guid? TutorSubjectId { get; set; }
        public Guid SlotId { get; set; }
        public DateTime SelectedDate { get; set; }
    }
}
