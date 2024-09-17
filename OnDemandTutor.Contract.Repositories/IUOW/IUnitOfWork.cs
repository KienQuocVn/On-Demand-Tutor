﻿using OnDemandTutor.Contract.Repositories.Entity;
using OnDemandTutor.Contract.Repositories.IUOW;

namespace OnDemandTutor.Contract.Repositories.Interface
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<T> GetRepository<T>() where T : class;
        IGenericRepository<TutorSubject> TutorRepository { get; }
        void Save();
        Task SaveAsync();
        void BeginTransaction();
        void CommitTransaction();
        void RollBack();
    }
}
