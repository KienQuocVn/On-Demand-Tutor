using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnDemandTutor.ModelViews.SLotModelViews
{
    public class UpdateSlotModelView
    {
        public Guid ClassId { get; set; }
        public string DayOfSlot { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public double Price { get; set; }
    }
}
