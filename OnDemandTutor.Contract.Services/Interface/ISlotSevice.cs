using OnDemandTutor.Contract.Repositories.Entity;
using OnDemandTutor.Core.Base;
using OnDemandTutor.ModelViews.SLotModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnDemandTutor.Contract.Services.Interface
{
    public interface ISlotSevice
    {
        public Task<BasePaginatedList<Slot>> GetAllSlotByFilterAsync(int pageNumber,int pageSize, string? id, string? classId,TimeSpan? StartTime, TimeSpan? endTime, double? price);
        public Task<Slot> CreateSlotAsync(SlotModelView model);
        public Task<Slot> UpdateSlotAsync(string id,SlotModelView model);
        public Task<bool> DeleteSlotAsync(string id);
    }
}
