using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnDemandTutor.Contract.Repositories.Entity;
using OnDemandTutor.Contract.Repositories.Interface;
using OnDemandTutor.Contract.Services.Interface;
using OnDemandTutor.Core.Base;
using OnDemandTutor.ModelViews.FeedbackModelViews;
using OnDemandTutor.ModelViews.ScheduleModelViews;
using OnDemandTutor.Repositories.Entity;
using OnDemandTutor.Repositories.UOW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnDemandTutor.Services.Service
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IUnitOfWork _unitOfWork;
        private IMapper _mapper;
        private readonly UserManager<Accounts> _userManager;

        public FeedbackService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<Accounts> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager; // Inject UserManager<Accounts>
        }

        public async Task<Feedback> CreateFeedbackAsync(CreateFeedbackModelViews model)
        {
            // Tìm user dựa trên StudentId
            var student = await _userManager.Users.FirstOrDefaultAsync(s => s.Id == model.StudentId && !s.DeletedTime.HasValue);

            // kiểm tra student có tồi tại hoặc đúng vai trò
            if (student == null || !await _userManager.IsInRoleAsync(student, "Student"))
            {
                throw new Exception("Không tìm thấy sinh viên! hãy thử lại");
            }

            // Tìm user dựa trên TutorId
            var tutor = await _userManager.Users.FirstOrDefaultAsync(s => s.Id == model.TutorId && !s.DeletedTime.HasValue);

            // kiểm tra tutor có tồi tại hoặc đúng vai trò
            if (tutor == null || !await _userManager.IsInRoleAsync(tutor, "Tutor"))
            {
                throw new Exception("Không tìm thấy gia sư! hãy thử lại");
            }

            //Kiểm tra sự tồn tại của Slot
            bool isExistSlot = await _unitOfWork.GetRepository<Slot>().Entities
                .AnyAsync(s => s.Id == model.SlotId && !s.DeletedTime.HasValue);

            if (!isExistSlot)
            {
                throw new Exception("Không tìm thấy Buổi học! Hãy thử lại");
            }

            //Kiểm tra sự tồn tại của Class
            bool isExistClass = await _unitOfWork.GetRepository<Class>().Entities
                .AnyAsync(s => s.Id == model.ClassId && !s.DeletedTime.HasValue);

            if (!isExistClass)
            {
                throw new Exception("Không tìm thấy Lớp học! Hãy thử lại");
            }

            var feedback = _mapper.Map<Feedback>(model);


            // Kiểm tra xem Student đã feedback cho Tutor này chưa
            var feedbackExists = await _unitOfWork.FeedbackRepository.Entities
                .AnyAsync(f => f.StudentId == model.StudentId &&
                f.TutorId == model.TutorId &&
               !f.DeletedTime.HasValue &&
                f.SlotId == model.SlotId &&
                f.ClassId == model.ClassId);
            var feedbackHasDelete = await _unitOfWork.FeedbackRepository.Entities
                .FirstOrDefaultAsync(f => f.StudentId == model.StudentId &&
                f.TutorId == model.TutorId &&
                f.DeletedTime.HasValue &&
                f.SlotId == model.SlotId&&
                f.ClassId == model.ClassId);

            if (feedbackHasDelete != null) {
                feedbackHasDelete.DeletedTime = null;
                feedbackHasDelete.CreatedTime = DateTimeOffset.Now;
                await _unitOfWork.SaveAsync();
                return feedbackHasDelete;
            }

            if (feedbackExists)
            {
                throw new Exception("Sinh viên đã phản hồi đến gia sư này.");
            }
            // Thiết lập các thuộc tính còn lại
            feedback.Id = Guid.NewGuid(); 
            feedback.CreatedTime = DateTimeOffset.Now;
            feedback.LastUpdatedTime = DateTimeOffset.Now;

            // Thêm thực thể Feedback vào cơ sở dữ liệu
            await _unitOfWork.FeedbackRepository.InsertAsync(feedback);
            await _unitOfWork.SaveAsync();

            return feedback;
        }

        public async Task<BasePaginatedList<Feedback>> GetDeleteAtFeedbackAsync(int pageNumber, int pageSize, string? slotId,string? classId, Guid? studentId, Guid? tutorId, Guid? feedbackId)
        {
            IQueryable<Feedback> feedbackQuery = _unitOfWork.GetRepository<Feedback>().Entities
                .Where(p => p.DeletedTime.HasValue)  // Lấy feedback đã bị xóa mềm
                .OrderByDescending(p => p.CreatedTime);

            // Điều kiện tìm kiếm theo StudentId
            if (studentId.HasValue && studentId != Guid.Empty)
            {
                feedbackQuery = feedbackQuery.Where(p => p.StudentId == studentId.Value);

                if (!await feedbackQuery.AnyAsync())
                {
                    throw new KeyNotFoundException("Không tìm thấy Feedback với StudentId.");
                }
            }

            // Điều kiện tìm kiếm theo TutorId
            if (tutorId.HasValue && tutorId != Guid.Empty)
            {
                feedbackQuery = feedbackQuery.Where(p => p.TutorId == tutorId.Value);

                if (!await feedbackQuery.AnyAsync())
                {
                    throw new KeyNotFoundException("Không tìm thấy Feedback với TutorId.");
                }
            }

            // Điều kiện tìm kiếm theo SlotId
            if (!string.IsNullOrEmpty(slotId))
            {
                feedbackQuery = feedbackQuery.Where(p => p.Slot.Id == slotId);

                if (!await feedbackQuery.AnyAsync())
                {
                    throw new KeyNotFoundException("Không tìm thấy Feedback với SlotId.");
                }
            }

            // Điều kiện tìm kiếm theo ClassId
            if (!string.IsNullOrEmpty(classId))
            {
                feedbackQuery = feedbackQuery.Where(p => p.Class.Id == classId);

                if (!await feedbackQuery.AnyAsync())
                {
                    throw new KeyNotFoundException("Không tìm thấy Feedback với ClassId.");
                }
            }

            // Điều kiện tìm kiếm theo FeedbackId
            if (feedbackId.HasValue && feedbackId != Guid.Empty)
            {
                feedbackQuery = feedbackQuery.Where(p => p.Id == feedbackId.Value);

                if (!await feedbackQuery.AnyAsync())
                {
                    throw new KeyNotFoundException("Không tìm thấy Feedback với FeedbackId.");
                }
            }
            // Tính tổng số lượng bản ghi sau khi lọc
            int totalCount = await feedbackQuery.CountAsync();
            //phân trang
            var feedback = await feedbackQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return new BasePaginatedList<Feedback>(feedback, totalCount, pageNumber, pageSize);
        }

        public async Task<BasePaginatedList<Feedback>> GetFeedbackByFilterAsync(int pageNumber, int pageSize, string? slotId, string? classId, Guid? studentId, Guid? tutorId, Guid? feedbackId)
        {
            IQueryable<Feedback> feedbackQuery = _unitOfWork.GetRepository<Feedback>().Entities
                .Where(p => !p.DeletedTime.HasValue);  // Lọc feedback chưa bị xóa mềm

            // Điều kiện tìm kiếm theo StudentId
            if (studentId.HasValue && studentId != Guid.Empty)
            {
                feedbackQuery = feedbackQuery.Where(p => p.StudentId == studentId.Value);

                if (!await feedbackQuery.AnyAsync())
                {
                    throw new KeyNotFoundException("Không tìm thấy Feedback với StudentId.");
                }
            }

            // Điều kiện tìm kiếm theo TutorId
            if (tutorId.HasValue && tutorId != Guid.Empty)
            {
                feedbackQuery = feedbackQuery.Where(p => p.TutorId == tutorId.Value);

                if (!await feedbackQuery.AnyAsync())
                {
                    throw new KeyNotFoundException("Không tìm thấy Feedback với TutorId.");
                }
            }

            // Điều kiện tìm kiếm theo SlotId
            if (!string.IsNullOrEmpty(slotId))
            {
                feedbackQuery = feedbackQuery.Where(p => p.Slot.Id == slotId);

                if (!await feedbackQuery.AnyAsync())
                {
                    throw new KeyNotFoundException("Không tìm thấy Feedback với SlotId.");
                }
            }

            // Điều kiện tìm kiếm theo ClassId
            if (!string.IsNullOrEmpty(classId))
            {
                feedbackQuery = feedbackQuery.Where(p => p.Class.Id == classId);

                if (!await feedbackQuery.AnyAsync())
                {
                    throw new KeyNotFoundException("Không tìm thấy Feedback với ClassId.");
                }
            }

            // Điều kiện tìm kiếm theo FeedbackId
            if (feedbackId.HasValue && feedbackId != Guid.Empty)
            {
                feedbackQuery = feedbackQuery.Where(p => p.Id == feedbackId.Value);

                if (!await feedbackQuery.AnyAsync())
                {
                    throw new KeyNotFoundException("Không tìm thấy Feedback với FeedbackId.");
                }
            }

            // Tính tổng số lượng bản ghi sau khi lọc
            int totalCount = await feedbackQuery.CountAsync();

            // Phân trang
            var feedback = await feedbackQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new BasePaginatedList<Feedback>(feedback, totalCount, pageNumber, pageSize);
        }



        public async Task<Feedback> UpdateFeedbackAsync(Guid id, Guid studentId, UpdateFeedbackModelViews model)
        {
            if (id == Guid.Empty)
            {
                throw new Exception("Chưa nhập ID Feedback.");
            }
            if (studentId == Guid.Empty) {
                throw new Exception("Chưa nhập ID sinh viên.");
            }
            // Tìm feedback dựa trên feedbackId và studentId
            var feedback = await _unitOfWork.FeedbackRepository.Entities
                .FirstOrDefaultAsync(f => f.Id == id && f.StudentId == studentId && !f.DeletedTime.HasValue);

            var student = await _unitOfWork.FeedbackRepository.Entities
              .FirstOrDefaultAsync(f => f.StudentId == studentId && !f.DeletedTime.HasValue);
            if (student == null)
            {
                throw new KeyNotFoundException("Không tìm thấy student! Hãy thử lại.");
            }

            // Nếu feedback không tồn tại hoặc không phải của sinh viên này, trả về thông báo lỗi
            if (feedback == null)
            {
                throw new KeyNotFoundException("Không tìm thấy feedback cho sinh viên này! Hãy thử lại.");
            }
            // Kiểm tra xem nội dung có thay đổi hay không
            if (feedback.FeedbackText == model.FeedbackText)
            {
                throw new InvalidOperationException("Nội dung không có sự thay đổi");
            }
            // Sử dụng AutoMapper để ánh xạ dữ liệu
            _mapper.Map(model, feedback);
            feedback.LastUpdatedTime = DateTimeOffset.Now; // Cập nhật thời gian sửa đổi

            await _unitOfWork.SaveAsync();

            return feedback;
        }




        public async Task<bool> DeleteFeedbackAsync(Guid id, Guid studentId)
        {
            // kiểm tra đã nhập hay chưa
            if (id == Guid.Empty)
            {
                throw new Exception("Chưa nhập ID Feedback.");
            }
            if (studentId == Guid.Empty)
            {
                throw new Exception("Chưa nhập ID sinh viên.");
            }
            // tìm kiếm id feedback chưa bị xóa
            var existingFeedback = await _unitOfWork.FeedbackRepository.Entities
            .FirstOrDefaultAsync(f => f.Id == id && f.StudentId == studentId && !f.DeletedTime.HasValue);
            //nếu chưa tiến hành xóa
            if (existingFeedback != null)
            {
                existingFeedback.DeletedTime = DateTimeOffset.Now;
                _unitOfWork.FeedbackRepository.Update(existingFeedback);
                await _unitOfWork.SaveAsync();

                return true; // Trả về true nếu xóa thành công
            }
            else {
                throw new Exception("Không tìm thấy feedback hoặc đã bị xóa!!");
            }

            return false;
        }

    }
}