
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnDemandTutor.ModelViews.Booking
{
    public class TimeBookingDto
    {
        public Guid StudentId { get; set; }
        public Guid SubjectId { get; set; }
        public Guid TutorSubjectId { get; set; }
        public DateTime SelectedDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
