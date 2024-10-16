using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnDemandTutor.Contract.Repositories.Entity;
using OnDemandTutor.Contract.Repositories.Interface;
using OnDemandTutor.Contract.Services.Interface;
using OnDemandTutor.Core.Base;
using OnDemandTutor.ModelViews.ScheduleModelViews;
using OnDemandTutor.Repositories.Entity;
using OnDemandTutor.Repositories.UOW;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnDemandTutor.Services.Service
{
    public class ScheduleService : IScheduleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;



        public ScheduleService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;

        }


        // Lấy danh sách Tất cả Schedule (xóa và chưa bị xóa) dựa theo các tham số truyền vào (nếu tham số nào null thì k tìm theo tham số đó)
        public async Task<BasePaginatedList<Schedule>> GetAllSchedulesAsync(int pageNumber, int pageSize, Guid? studentId, Guid? slotId, string? status)
        {
            // Lấy tất cả các bản ghi trong bảng Schedule với điều kiện tìm kiếm
            IQueryable<Schedule> schedulesQuery = _unitOfWork.GetRepository<Schedule>().Entities
                .OrderByDescending(p => p.CreatedTime);

            // Điều kiện tìm kiếm theo studentId nếu có
            if (studentId.HasValue)
            {
                schedulesQuery = schedulesQuery.Where(p => p.StudentId == studentId);
            }

            // Điều kiện tìm kiếm theo slotId nếu có
            if (slotId.HasValue)
            {
                schedulesQuery = schedulesQuery.Where(p => p.SlotId == slotId);
            }

            // Điều kiện tìm kiếm theo status nếu có
            if (!string.IsNullOrWhiteSpace(status))
            {
                schedulesQuery = schedulesQuery.Where(p => p.Status == status);
            }

            // Đếm tổng số bản ghi
            int totalCount = await schedulesQuery.CountAsync();

            // Áp dụng phân trang và sắp xếp theo Id
            List<Schedule> paginatedSchedules = await schedulesQuery
                .OrderBy(s => s.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new BasePaginatedList<Schedule>(paginatedSchedules, totalCount, pageNumber, pageSize);
        }

        // Lấy danh sách Schedule chưa bị xóa dựa theo các tham số truyền vào (nếu tham số nào null thì k tìm theo tham số đó)
        public async Task<BasePaginatedList<Schedule>> GetSchedulesByFilterAsync(int pageNumber, int pageSize, Guid? studentId, Guid? slotId, string? status)
        {
            // Lấy tất cả các bản ghi trong bảng Schedule với điều kiện tìm kiếm
            IQueryable<Schedule> schedulesQuery = _unitOfWork.GetRepository<Schedule>().Entities
                .Where(p => !p.DeletedTime.HasValue || string.IsNullOrEmpty(p.DeletedBy))
                .OrderByDescending(p => p.CreatedTime);

            // Điều kiện tìm kiếm theo studentId nếu có
            if (studentId.HasValue)
            {
                schedulesQuery = schedulesQuery.Where(p => p.StudentId == studentId);
            }

            // Điều kiện tìm kiếm theo slotId nếu có
            if (slotId.HasValue)
            {
                schedulesQuery = schedulesQuery.Where(p => p.SlotId == slotId);
            }

            // Điều kiện tìm kiếm theo status nếu có
            if (!string.IsNullOrWhiteSpace(status))
            {
                schedulesQuery = schedulesQuery.Where(p => p.Status == status);
            }

            // Đếm tổng số bản ghi
            int totalCount = await schedulesQuery.CountAsync();

            // Áp dụng phân trang và sắp xếp theo Id
            List<Schedule> paginatedSchedules = await schedulesQuery
                .OrderBy(s => s.Id) 
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new BasePaginatedList<Schedule>(paginatedSchedules, totalCount, pageNumber, pageSize);
        }
        
        // Tạo 1 Schedule mới với tham số chuyền vào là studentID, SlotID, Status
        public async Task<ResponseScheduleModelViews> CreateScheduleAsync(CreateScheduleModelViews model)
        {
            // Kiểm tra các thuộc tính không được để trống


            if (model.StudentId == Guid.Empty)
            {
                throw new Exception("Please enter StudentId.");
            }

            if (model.SlotId == Guid.Empty)
            {
                throw new Exception("Please enter SlotId.");
            }

            if (string.IsNullOrWhiteSpace(model.Status))
            {
                throw new Exception("Please enter Status.");
            }

            // Kiểm tra sự tồn tại của Student
            bool isExistStudent = await _unitOfWork.GetRepository<Accounts>().Entities
                .AnyAsync(s => s.Id == model.StudentId && !s.DeletedTime.HasValue);

            if (!isExistStudent)
            {
                throw new Exception("The Student can not found or has been deleted!");
            }

            // Kiểm tra sự tồn tại của Slot
            bool isExistSlot = await _unitOfWork.GetRepository<Slot>().Entities
                .AnyAsync(s => s.Id == model.SlotId && !s.DeletedTime.HasValue);

            if (!isExistSlot)
            {
                throw new Exception("The Slot can not found or has been deleted!");
            }

            // Kiểm tra xem Schedule đã tồn tại với StudentId và SlotId chưa
            Schedule existingSchedule = await _unitOfWork.GetRepository<Schedule>().Entities
                .FirstOrDefaultAsync(p => p.StudentId == model.StudentId && p.SlotId == model.SlotId && !p.DeletedTime.HasValue);

            if (existingSchedule != null)
            {
                throw new Exception("The Schedule already exists and cannot be created again!");
            }

            // Sử dụng AutoMapper để ánh xạ từ model sang thực thể Schedule
            Schedule schedule = _mapper.Map<Schedule>(model);

            // Thiết lập thêm các thuộc tính không có trong CreateScheduleModelViews
            schedule.Id = Guid.NewGuid();
            schedule.CreatedBy = "claim account";  // Ví dụ: lấy từ thông tin xác thực
            schedule.CreatedTime = DateTimeOffset.UtcNow;
            schedule.LastUpdatedTime = DateTimeOffset.UtcNow;

            // Thêm thực thể Schedule vào cơ sở dữ liệu
            await _unitOfWork.ScheduleRepository.InsertAsync(schedule);
            await _unitOfWork.SaveAsync();

            return _mapper.Map<ResponseScheduleModelViews>(schedule);
        }


        // cập nhật lịch tham số truyền vào là studentID, SlotId, Status
        public async Task<ResponseScheduleModelViews> UpdateScheduleAsync(Guid studentId, Guid slotId, UpdateScheduleModelViews model)
        {
            if (studentId == Guid.Empty)
            {
                throw new Exception("Please enter StudentId.");
            }

            if (slotId == Guid.Empty)
            {
                throw new Exception("Please enter SlotId.");
            }

            // Kiểm tra sự tồn tại của Student
            bool isExistStudent = await _unitOfWork.GetRepository<Accounts>().Entities
                .AnyAsync(s => s.Id == model.StudentId && !s.DeletedTime.HasValue);

            if (!isExistStudent)
            {
                throw new Exception("The Student cannot be found or has been deleted!");
            }

            // Kiểm tra sự tồn tại của Slot
            bool isExistSlot = await _unitOfWork.GetRepository<Slot>().Entities
                .AnyAsync(s => s.Id == model.SlotId && !s.DeletedTime.HasValue);

            if (!isExistSlot)
            {
                throw new Exception("The Slot cannot be found or has been deleted!");
            }

            // Kiểm tra tính hợp lệ của StudentId và SlotId trước khi truy vấn
            if (studentId == Guid.Empty)
            {
                throw new Exception("StudentId is invalid.");
            }

            if (slotId ==Guid.Empty)
            {
                throw new Exception("SlotId is invalid.");
            }


            // Truy vấn để tìm schedule từ database dựa trên StudentId và SlotId
            Schedule existingSchedule = await _unitOfWork.GetRepository<Schedule>().Entities
                .FirstOrDefaultAsync(p => p.StudentId == studentId && p.SlotId == slotId && !p.DeletedTime.HasValue)
                ?? throw new Exception("The Schedule cannot be found or deleted!");

            // Kiểm tra sự tồn tại và sự thay đổi của Schedule
            if (existingSchedule.SlotId == model.SlotId &&
                existingSchedule.StudentId == model.StudentId &&
                existingSchedule.Status == model.Status)
            {
                throw new Exception("The Schedule does not have any changes.");
            }

            // Sử dụng AutoMapper để cập nhật dữ liệu từ model vào thực thể Schedule
            _mapper.Map(model, existingSchedule);

            // Thiết lập các thuộc tính bổ sung
            existingSchedule.LastUpdatedBy = "claim account";  // Ví dụ: lấy từ thông tin xác thực
            existingSchedule.LastUpdatedTime = DateTimeOffset.UtcNow;

            // Cập nhật thực thể Schedule vào database
            _unitOfWork.ScheduleRepository.Update(existingSchedule);
            await _unitOfWork.SaveAsync();

            return _mapper.Map<ResponseScheduleModelViews>(existingSchedule);
        }

        // xóa mềm 1 lịch truyền vào studentID, SlotId
        public async Task<ResponseScheduleModelViews> DeleteScheduleAsync(Guid studentId, Guid slotId)
        {
            // Validate đầu vào: Kiểm tra xem StudentId và SlotId có hợp lệ hay không
            if (studentId == Guid.Empty)
            {
                throw new Exception("Please provide a valid Student ID.");
            }

            if (slotId == Guid.Empty)
            {
                throw new Exception("Please provide a valid Slot ID.");
            }

            // Lấy schedule từ database dựa trên StudentId và SlotId
            Schedule existingSchedule = await _unitOfWork.GetRepository<Schedule>().Entities
                .FirstOrDefaultAsync(p => p.StudentId == studentId && p.SlotId == slotId && !p.DeletedTime.HasValue)
                ?? throw new Exception("The Schedule cannot be found or it has been deleted!");

            // Thực hiện xóa mềm
            existingSchedule.DeletedTime = DateTimeOffset.UtcNow;
            existingSchedule.DeletedBy = "claim account"; // Có thể thay thế bằng thông tin người dùng đăng nhập

            // Cập nhật thực thể Schedule trong cơ sở dữ liệu
            _unitOfWork.ScheduleRepository.Update(existingSchedule);
            await _unitOfWork.SaveAsync();

            return _mapper.Map<ResponseScheduleModelViews>(existingSchedule);
        }

    }
}
