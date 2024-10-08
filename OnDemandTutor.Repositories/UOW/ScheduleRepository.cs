﻿using Microsoft.EntityFrameworkCore;
using OnDemandTutor.Contract.Repositories.Entity;
using OnDemandTutor.Contract.Repositories.IUOW;
using OnDemandTutor.Repositories.Context;

namespace OnDemandTutor.Repositories.UOW
{
    public class ScheduleRepository : GenericRepository<Schedule>, IScheduleRepository
    {
        protected readonly DatabaseContext _context;
        protected readonly DbSet<Schedule> _dbSet;
        public ScheduleRepository(DatabaseContext dbContext) : base(dbContext)
        {
            _context = dbContext;
            _dbSet = _context.Set<Schedule>();
        }
    }
}
