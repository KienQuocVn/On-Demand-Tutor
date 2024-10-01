using OnDemandTutor.Contract.Repositories.Entity;
using OnDemandTutor.Contract.Repositories.Interface;
using OnDemandTutor.Contract.Repositories.IUOW;
using OnDemandTutor.Repositories.Context;
using System;

namespace OnDemandTutor.Repositories.UOW
{
    public class UnitOfWork : IUnitOfWork
    {
        private bool disposed = false;
        private readonly DatabaseContext _dbContext;

        public UnitOfWork(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        private IGenericRepository<Schedule> scheduleRepository;
        public IGenericRepository<Schedule> ScheduleRepository
        {
            get
            {
                if (scheduleRepository == null)
                {
                    scheduleRepository = new GenericRepository<Schedule>(_dbContext);
                }
                return scheduleRepository;
            }
        }

        private IGenericRepository<TutorSubject> tutorRepository;
        public IGenericRepository<TutorSubject> TutorRepository
        {
            get
            {
                if (tutorRepository == null)
                {
                    tutorRepository = new GenericRepository<TutorSubject>(_dbContext);
                }
                return tutorRepository;
            }
        }

        private IGenericRepository<Feedback> feedbackRepository;
        public IGenericRepository<Feedback> FeedbackRepository
        {
            get
            {
                if (feedbackRepository == null)
                {
                    feedbackRepository = new GenericRepository<Feedback>(_dbContext);
                }
                return feedbackRepository;
            }
        }
        public IGenericRepository<Slot> slotRepository;

        public IGenericRepository<Slot> SlotRepository
        {
            get
            {
                if (this.slotRepository == null)
                {
                    this.slotRepository = new GenericRepository<Slot>(_dbContext);
                }
                return slotRepository;
            }
        }

        private IGenericRepository<Subject> subjectRepository;
        public IGenericRepository<Subject> SubjectRepository
        {
            get
            {
                if (subjectRepository == null)
                {
                    subjectRepository = new GenericRepository<Subject>(_dbContext);
                }
                return subjectRepository;
            }
        }

        private IGenericRepository<Complaint> complaintRepository;
        public IGenericRepository<Complaint> ComplaintRepository
        {
            get
            {
                if (complaintRepository == null)
                {
                    complaintRepository = new GenericRepository<Complaint>(_dbContext);
                }
                return complaintRepository;
            }
        }


        public IGenericRepository<Class> classRepository;
        public IGenericRepository<Class> ClassRepository
        {
            get
            {
                if (this.classRepository == null)
                {
                    this.classRepository = new GenericRepository<Class>(_dbContext);
                }
                return classRepository;
            }
        }

        public void BeginTransaction()

        {
            _dbContext.Database.BeginTransaction();
        }

        public void CommitTransaction() // Cam kết giao dịch
        {
            _dbContext.Database.CommitTransaction();
        }

        public void Dispose() // Giải phóng tài nguyên
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _dbContext.Dispose();
                }
            }
            disposed = true;
        }

        public void RollBack()
        {
            _dbContext.Database.RollbackTransaction();
        }

        public void Save()
        {
            _dbContext.SaveChanges();
        }

        public async Task SaveAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

        public IGenericRepository<T> GetRepository<T>() where T : class
        {
            return new GenericRepository<T>(_dbContext);
        }
    }
}
