using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnDemandTutor.Contract.Repositories.Entity;
using OnDemandTutor.Contract.Repositories.Interface;
using OnDemandTutor.Contract.Services.Interface;
using OnDemandTutor.Core.Base;
using OnDemandTutor.ModelViews.SLotModelViews;
using OnDemandTutor.Repositories.Entity;
using OnDemandTutor.Repositories.UOW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnDemandTutor.Services.Service
{
    public class SlotService : ISlotSevice
    {
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;

        public SlotService(IUnitOfWork unitOfWork, IMapper mapper) { 
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<Slot> CreateSlotAsync(SlotModelView model)
        {
            bool isExistClass = await _unitOfWork.GetRepository<Class>().Entities
                .AnyAsync(s => s.Id == model.ClassId && !s.DeletedTime.HasValue);
            // Kiểm tra class có tồn tại hay không
            if (!isExistClass) {
                throw new Exception("Không tìm thấy class! Hãy thử lại");
            }

            // Kiểm tra xem StartTime có nhỏ hơn EndTime không
            if (model.StartTime >= model.EndTime)
            {
                throw new ArgumentException("StartTime phải nhỏ hơn EndTime.");
            }
            // Kiểm tra trùng thời gian với slot khác trong cùng class
            bool isDuplicateTime = await _unitOfWork.SlotRepository.Entities
                .AnyAsync(s => s.ClassId == model.ClassId
                               && s.StartTime == model.StartTime
                               && s.EndTime == model.EndTime
                               && !s.DeletedTime.HasValue);

            if (isDuplicateTime)
            {
                throw new Exception("Đã tồn tại slot với thời gian trùng lặp trong class này!");
            }

            var slot = _mapper.Map<Slot>(model);

            if (slot == null)
            {
                throw new Exception("Lỗi khi map dữ liệu từ SlotModelView sang Slot.");
            }
            // Thiết lập các thuộc tính còn lại
            slot.Id = Guid.NewGuid().ToString("N");
            slot.CreatedBy = "admin";  // Ví dụ: lấy từ thông tin xác thực
            slot.CreatedTime = DateTimeOffset.Now;
            slot.LastUpdatedTime = DateTimeOffset.Now;

            // Thêm thực thể slot vào cơ sở dữ liệu
            await _unitOfWork.SlotRepository.InsertAsync(slot);
            // Kiểm tra slot trước khi lưu
            Console.WriteLine($"SlotId: {slot.Id}, StartTime: {slot.StartTime}, EndTime: {slot.EndTime}");

            await _unitOfWork.SaveAsync();

            return slot;
        }
        public async Task<BasePaginatedList<Slot>> GetAllSlotByFilterAsync(int pageNumber, int pageSize, string? id, string? classId, TimeSpan? StartTime, TimeSpan? endTime, double? price)
        {
            IQueryable<Slot> SlotQuerys = _unitOfWork.GetRepository<Slot>().Entities
                           .Where(p => !p.DeletedTime.HasValue) // Lọc Slot chưa bị xóa mềm
                           .OrderByDescending(p => p.CreatedTime);

            // Điều kiện tìm kiếm theo id slot
            if (!string.IsNullOrEmpty(id))
            {
                SlotQuerys = SlotQuerys.Where(p => p.Id == id);

                if (!await SlotQuerys.AnyAsync())
                {
                    throw new KeyNotFoundException("Không tìm thấy ID Slot");
                }
            }
            // Điều kiện tìm kiếm theo classId
            if (!string.IsNullOrEmpty(classId))
            {
                SlotQuerys = SlotQuerys.Where(p => p.ClassId == classId);

                if (!await SlotQuerys.AnyAsync())
                {
                    throw new KeyNotFoundException("Không tìm thấy Slot với classId");
                }
            }

            // Điều kiện tìm kiếm theo StartTime
            if (StartTime.HasValue && !endTime.HasValue)
            {
                SlotQuerys = SlotQuerys.Where(p => p.StartTime >= StartTime.Value);

                if (!await SlotQuerys.AnyAsync())
                {
                    throw new KeyNotFoundException($"Không tìm thấy Slot với StartTime lớn hơn hoặc bằng: {StartTime.Value}.");
                }
            }

            // Điều kiện tìm kiếm theo EndTime
            if (endTime.HasValue && !StartTime.HasValue)
            {
                SlotQuerys = SlotQuerys.Where(p => p.EndTime <= endTime.Value);

                if (!await SlotQuerys.AnyAsync())
                {
                    throw new KeyNotFoundException($"Không tìm thấy Slot với EndTime nhỏ hơn hoặc bằng: {endTime.Value}.");
                }
            }

            // Điều kiện tìm kiếm khi cả StartTime và EndTime đều có giá trị
            if (endTime.HasValue && StartTime.HasValue)
            {
                SlotQuerys = SlotQuerys.Where(p => p.StartTime >= StartTime.Value && p.EndTime <= endTime.Value);

                if (!await SlotQuerys.AnyAsync())
                {
                    throw new KeyNotFoundException($"Không tìm thấy Slot khoảng thời gian từ {StartTime.Value} đến {endTime.Value}.");
                }
            }

            // Điều kiện tìm kiếm theo giá
            if (price.HasValue)
            {
                SlotQuerys = SlotQuerys.Where(p => p.Price == price.Value);

                if (!await SlotQuerys.AnyAsync())
                {
                    throw new KeyNotFoundException($"Không tìm thấy Slot với giá: {price.Value}.");
                }
            }

            // Tính tổng số lượng bản ghi sau khi lọc
            int totalCount = await SlotQuerys.CountAsync();

            // Phân trang
            var slot = await SlotQuerys
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new BasePaginatedList<Slot>(slot, totalCount, pageNumber, pageSize);
        }



        public async Task<Slot> UpdateSlotAsync(string id, SlotModelView model)
        {

            var slot = await _unitOfWork.SlotRepository.Entities
                .FirstOrDefaultAsync(s => s.Id == id && !s.DeletedTime.HasValue);

            // kiểm tra slot có tồn tại hay không
            if (slot == null) {
                throw new Exception("Không tìm thấy slot! Hãy thử lại");
            }
            // Kiểm tra classId có tồn tại hay không
            var classEntity = await _unitOfWork.GetRepository<Class>().Entities
                .FirstOrDefaultAsync(c => c.Id == model.ClassId);

            if (classEntity == null)
            {
                throw new KeyNotFoundException("Không tìm thấy class với ID đã cho.");
            }

            // Kiểm tra xem StartTime có nhỏ hơn EndTime không
            if (model.StartTime >= model.EndTime)
            {
                throw new ArgumentException("StartTime phải nhỏ hơn EndTime.");
            }

            // Kiểm tra trùng thời gian với slot khác trong cùng class
            bool isDuplicateTime = await _unitOfWork.SlotRepository.Entities
                .AnyAsync(s => s.ClassId == model.ClassId
                               && s.StartTime == model.StartTime
                               && s.EndTime == model.EndTime
                               && s.Id != id // Đảm bảo không so sánh với slot hiện tại
                               && !s.DeletedTime.HasValue);

            if (isDuplicateTime)
            {
                throw new Exception("Đã tồn tại slot với thời gian trùng lặp trong class này!");
            }

            _mapper.Map(model, slot);
            slot.LastUpdatedBy = "admin"; // ví dụ: lấy thông tin từ xác thực
            slot.LastUpdatedTime = DateTimeOffset.Now; // Cập nhật thời gian sửa đổi

            await _unitOfWork.SaveAsync();

            return slot;
        }

        public async Task<bool> DeleteSlotAsync(string id)
        {
            var slot = await _unitOfWork.SlotRepository.Entities
                .FirstOrDefaultAsync(s => s.Id == id && !s.DeletedTime.HasValue);

            if (slot != null)
            {
                slot.DeletedTime = DateTimeOffset.Now;
                _unitOfWork.SlotRepository.Update(slot);
                await _unitOfWork.SaveAsync();

                return true; // Trả về true nếu xóa thành công
            }
            else
            {
                throw new Exception("Không tìm thấy Slot hoặc đã bị xóa!");
            }
            return false;
        }
    }
}
