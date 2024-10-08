﻿using Microsoft.EntityFrameworkCore;
using OnDemandTutor.Contract.Repositories.Entity;
using OnDemandTutor.Repositories.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnDemandTutor.Repositories.UOW
{
    public class SubjectRepository : GenericRepository<Subject>
    {
        // Khai báo biến để lưu trữ ngữ cảnh cơ sở dữ liệu  
        protected readonly DatabaseContext _context;

        // Khai báo biến để lưu trữ tập hợp TutorSubject trong cơ sở dữ liệu  
        protected readonly DbSet<Subject> _dbSet;

        // Constructor nhận vào DatabaseContext và truyền cho lớp cơ sở thông qua base  
        public SubjectRepository(DatabaseContext dbContext) : base(dbContext)
        {
            // Gán ngữ cảnh cơ sở dữ liệu cho biến _context  
            _context = dbContext;

            // Thiết lập _dbSet để làm việc với các thực thể TutorSubject  
            _dbSet = _context.Set<Subject>();
        }
    }
}
