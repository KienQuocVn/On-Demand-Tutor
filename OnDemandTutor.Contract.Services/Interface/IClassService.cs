using OnDemandTutor.Contract.Repositories.Entity;
using OnDemandTutor.Core.Base;
using OnDemandTutor.ModelViews.ClassModelViews;
using OnDemandTutor.ModelViews.ScheduleModelViews;


namespace OnDemandTutor.Contract.Services.Interface
{
    public interface IClassService
    {
        Task<BasePaginatedList<Class>> GetAllClassesAsync(int pageNumber, int pageSize, Guid? classId, Guid? accountId, Guid? subjectId, DateTime? startDay, DateTime? endDay);
        Task<ResponseClassModelView> CreateClassAsync(CreateClassModelView model);
        Task<ResponseClassModelView> UpdateClassAsync(Guid id, UpdateClassModelView model);
        Task<ResponseClassModelView> DeleteClassAsync(Guid id);
        Task<Double> CalculateTotalAmount(Guid id);
    }
}
