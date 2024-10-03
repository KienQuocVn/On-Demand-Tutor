﻿using OnDemandTutor.Core.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnDemandTutor.Contract.Repositories.Entity
{
    public class Subject : BaseEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public Guid TutorId { get; set; }
        public string Name { get; set; }
        public virtual ICollection<TutorSubject> TutorSubjects { get; set; }
        public virtual ICollection<Class> Classes { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; }
    }
}
