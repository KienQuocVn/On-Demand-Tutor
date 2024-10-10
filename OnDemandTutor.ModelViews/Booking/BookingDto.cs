using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnDemandTutor.ModelViews.Booking
{
    
        public class BookingDto
        {
            public Guid StudentId { get; set; } // thay đổi
            public string SubjectId { get; set; }
            public string TutorSubjectId { get; set; }
        }
    

}
