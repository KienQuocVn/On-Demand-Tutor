using OnDemandTutor.Contract.Repositories.Entity;
using OnDemandTutor.Core.Base;
using OnDemandTutor.ModelViews.FeedbackModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnDemandTutor.Contract.Services.Interface
{
    public interface IFeedbackService
    {
        Task<BasePaginatedList<Feedback>> GetDeleteAtFeedbackAsync(int pageNumber, int pageSize, Guid? slotId, Guid? classId, Guid? studentId, Guid? tutorId, Guid? feedbackId);
        Task<BasePaginatedList<Feedback>> GetFeedbackByFilterAsync(int pageNumber, int pageSize, Guid? slotId, Guid? classId, Guid? studentId, Guid? tutorId, Guid? feedbackId);
        Task<Feedback> CreateFeedbackAsync(CreateFeedbackModelViews model);
        Task<Feedback> UpdateFeedbackAsync(Guid id, Guid studentId, UpdateFeedbackModelViews model);
        Task<bool> DeleteFeedbackAsync(Guid id, Guid studentId);
    }
}
