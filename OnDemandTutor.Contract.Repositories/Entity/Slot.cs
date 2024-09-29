﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnDemandTutor.Core.Base;

namespace OnDemandTutor.Contract.Repositories.Entity
{
    public class Slot : BaseEntity
    {
        //[Key]
        //public int Id { get; set; }

        //[ForeignKey("Class")]
        //public int ClassId { get; set; }

        //public string DayOfSlot { get; set; }
        //public TimeSpan StartTime { get; set; }
        //public TimeSpan EndTime { get; set; }
        //public int Price { get; set; }

        //// Navigation properties
        //public virtual Class Class { get; set; }
        //public int Id { get; set; }


        public string ClassId { get; set; }
        public string DayOfSlot { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public double Price { get; set; }

        // Navigation property
        public virtual Class Class { get; set; }
        public virtual ICollection<Schedule> Schedules { get; set; }

    }
}
