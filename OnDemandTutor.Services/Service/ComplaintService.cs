using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnDemandTutor.Contract.Repositories.Entity;
using OnDemandTutor.Contract.Repositories.Interface;
using OnDemandTutor.Contract.Repositories.IUOW;
using OnDemandTutor.Contract.Services.Interface;
using OnDemandTutor.Core.Base;
using OnDemandTutor.ModelViews.ComplaintModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnDemandTutor.Services.Service
{
    public class ComplaintService : IComplaintService
    {
        private readonly IUnitOfWork _unitOfWork; // Đơn vị công việc để xử lý các thao tác với cơ sở dữ liệu
        private readonly IMapper _mapper; // Mapper để ánh xạ giữa các đối tượng

        public ComplaintService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork; // Khởi tạo đơn vị công việc
            _mapper = mapper; // Khởi tạo mapper
        }

        // Lấy tất cả khiếu nại với phân trang
        public async Task<BasePaginatedList<Complaint>> GetAllComplaintsAsync(int pageNumber, int pageSize, Guid? studentId, Guid? tutorId, string? status)
        {
            try
            {
                IQueryable<Complaint> complaintsQuery = _unitOfWork.GetRepository<Complaint>().Entities
                    .OrderByDescending(c => c.CreatedAt);

                if (studentId.HasValue)
                {
                    complaintsQuery = complaintsQuery.Where(c => c.StudentId == studentId);
                }

                if (tutorId.HasValue)
                {
                    complaintsQuery = complaintsQuery.Where(c => c.TutorId == tutorId);
                }

                if (!string.IsNullOrWhiteSpace(status))
                {
                    complaintsQuery = complaintsQuery.Where(c => c.Status == status);
                }

                int totalCount = await complaintsQuery.CountAsync();
                List<Complaint> paginatedComplaints = await complaintsQuery
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new BasePaginatedList<Complaint>(paginatedComplaints, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while retrieving complaints: " + ex.Message);
            }
        }

        // Lấy một khiếu nại theo ID
        public async Task<ResponseComplaintModel> GetComplaintByIdAsync(Guid id)
        {
            try
            {
                var complaint = await _unitOfWork.GetRepository<Complaint>().Entities
                    .FirstOrDefaultAsync(c => c.Id == id)
                    ?? throw new Exception($"Complaint with ID {id} not found.");

                return _mapper.Map<ResponseComplaintModel>(complaint);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while retrieving complaint: " + ex.Message);
            }
        }

        // Tạo một khiếu nại mới
        public async Task<ResponseComplaintModel> CreateComplaintAsync(CreateComplaintModel model)
        {
            try
            {
                var complaint = _mapper.Map<Complaint>(model);
                complaint.Id = Guid.NewGuid();

                await _unitOfWork.GetRepository<Complaint>().InsertAsync(complaint);
                await _unitOfWork.SaveAsync();

                return _mapper.Map<ResponseComplaintModel>(complaint);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while creating complaint: " + ex.Message);
            }
        }

        // Cập nhật một khiếu nại theo ID
        public async Task<ResponseComplaintModel> UpdateComplaintAsync(Guid id, UpdateComplaintModel model)
        {
            try
            {
                var existingComplaint = await _unitOfWork.GetRepository<Complaint>().Entities
                    .FirstOrDefaultAsync(c => c.Id == id) ?? throw new Exception($"Complaint with ID {id} not found.");

                _mapper.Map(model, existingComplaint);
                await _unitOfWork.SaveAsync();

                return _mapper.Map<ResponseComplaintModel>(existingComplaint);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while updating complaint: " + ex.Message);
            }
        }

        // Xóa một khiếu nại theo ID
        public async Task<ResponseComplaintModel> DeleteComplaintAsync(Guid id)
        {
            try
            {
                var existingComplaint = await _unitOfWork.GetRepository<Complaint>().Entities
                    .FirstOrDefaultAsync(c => c.Id == id) ?? throw new Exception($"Complaint with ID {id} not found.");

                _unitOfWork.GetRepository<Complaint>().Delete(existingComplaint);
                await _unitOfWork.SaveAsync();

                return _mapper.Map<ResponseComplaintModel>(existingComplaint);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while deleting complaint: " + ex.Message);
            }
        }
    }
}
