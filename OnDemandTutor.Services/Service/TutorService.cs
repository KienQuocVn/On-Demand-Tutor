using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnDemandTutor.Contract.Repositories.Entity;
using OnDemandTutor.Contract.Repositories.Interface;
using OnDemandTutor.Contract.Services.Interface;
using OnDemandTutor.Core.Base;
using OnDemandTutor.ModelViews.ScheduleModelViews;
using OnDemandTutor.ModelViews.TutorSubjectModelViews;
using OnDemandTutor.Repositories.Entity;


namespace OnDemandTutor.Services.Service
{
    public class TutorService : ITutorService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        // Constructor nhận vào IUnitOfWork để khởi tạo service  
        public TutorService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork; // Gán giá trị cho biến _unitOfWork  
            _mapper = mapper;
        }

        // Phương thức lấy danh sách tất cả các môn học của gia sư với phân trang  
        public async Task<BasePaginatedList<TutorSubject>> GetAllTutor(int pageNumber, int pageSize, Guid? tutorId, Guid? subjectId)
        {
            IQueryable<TutorSubject> tutorQuery = _unitOfWork.GetRepository<TutorSubject>().Entities
                .OrderByDescending(p => p.CreatedTime);
            if (tutorId.HasValue)
            {
                tutorQuery = tutorQuery.Where(p => p.User.Id == tutorId);
            }

            if (subjectId.HasValue)
            {
                tutorQuery = tutorQuery.Where(p => p.Subject.Id == subjectId);
            }
            int totalCount = await tutorQuery.CountAsync();
            List<TutorSubject> tutorpage = await tutorQuery
                .OrderBy(s => s.TutorId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return new BasePaginatedList<TutorSubject>(tutorpage, totalCount, pageNumber, pageSize);
        }

        public async Task<BasePaginatedList<TutorSubject>> SearchById(int pageNumber, int pageSize, Guid? tutorId, Guid? subjectId)
        {
            // Lấy tất cả các bản ghi trong bảng Schedule với điều kiện tìm kiếm
            IQueryable<TutorSubject> tutorQuery = _unitOfWork.GetRepository<TutorSubject>().Entities
                .Where(p => !p.DeletedTime.HasValue || string.IsNullOrEmpty(p.DeletedBy))
                .OrderByDescending(p => p.CreatedTime);

            if (tutorId.HasValue)
            {
                tutorQuery = tutorQuery.Where(p => p.User.Id == tutorId);
            }

            if (subjectId.HasValue)
            {
                tutorQuery = tutorQuery.Where(p => p.User.Id == subjectId);
            }

            int totalCount = await tutorQuery.CountAsync();
            List<TutorSubject> tutorpage = await tutorQuery
                .OrderBy(s => s.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return new BasePaginatedList<TutorSubject>(tutorpage, totalCount, pageNumber, pageSize);
        }

        public async Task<ResponseTutorModelViews> CreateTutorSubjectAsync(CreateTutorSubjectModelViews model)
        {
            if (model.SubjectId == Guid.Empty)
            {
                throw new ArgumentException("Please enter a valid SubjectId.");
            }

            // Validate the Bio  
            if (string.IsNullOrWhiteSpace(model.Bio))
            {
                throw new ArgumentException("Please provide a Bio for the tutor.");
            }

            // Validate the Rating  
            if (model.Rating < 0 || model.Rating > 5)
            {
                throw new ArgumentException("Rating must be between 0 and 5.");
            }

            // Validate the HourlyRate  
            if (model.HourlyRate < 0)
            {
                throw new ArgumentException("HourlyRate cannot be negative.");
            }

            // Check if the subject exists  
            bool isExistSubject = await _unitOfWork.GetRepository<Subject>().Entities
                .AnyAsync(s => s.Id == model.SubjectId && !s.DeletedTime.HasValue);

            if (!isExistSubject)
            {
                throw new Exception("The Subject cannot be found or has been deleted!");
            }

            // Check if the TutorSubject already exists  
            TutorSubject existingTutor = await _unitOfWork.GetRepository<TutorSubject>().Entities
                .FirstOrDefaultAsync(p => p.UserId == model.UserId && p.SubjectId == model.SubjectId && !p.DeletedTime.HasValue);

            if (existingTutor != null)
            {
                throw new Exception("The TutorSubject already exists and cannot be created again!");
            }

            // Create a new TutorSubject entity  
            var tutorSubject = new TutorSubject
            {
                TutorId = Guid.NewGuid(),
                UserId = model.UserId,
                SubjectId = model.SubjectId,
                Bio = model.Bio,
                Rating = model.Rating,
                Experience = model.Experience,
                HourlyRate = model.HourlyRate,
                Id = Guid.NewGuid(),  
                CreatedBy = "admin",
                CreatedTime = DateTimeOffset.Now,
            };

            await _unitOfWork.TutorRepository.InsertAsync(tutorSubject);
            await _unitOfWork.SaveAsync();

            // Map the created TutorSubject back to a response model  
            return _mapper.Map<ResponseTutorModelViews>(tutorSubject);
        }

        public async Task<ResponseTutorModelViews> UpdateTutorSubjectAsync(Guid tutorId, Guid subjectId, UpdateTutorSubjectModelViews model)
        {
            // Kiểm tra xem tutorId có hợp lệ không  
            if (tutorId == Guid.Empty)
            {
                throw new ArgumentException("Please enter a valid tutorId.", nameof(tutorId));
            }
            // Kiểm tra xem subjectId có hợp lệ không  
            if (subjectId == Guid.Empty)
            {
                throw new ArgumentException("Please enter a valid subjectId.", nameof(subjectId));
            }

            // Kiểm tra xem gia sư có tồn tại không  
            //bool isExistTutor = await _unitOfWork.GetRepository<Accounts>().Entities
            //    .AnyAsync(s => s.Id == model.TutorId && !s.DeletedTime.HasValue);

            //if (!isExistTutor)
            //{
            //    throw new Exception("The Tutor can not found or has been deleted!");
            //}

            bool isExistSubject = await _unitOfWork.GetRepository<Subject>().Entities
                .AnyAsync(s => s.Id == model.SubjectId && !s.DeletedTime.HasValue);

            if (!isExistSubject)
            {
                throw new Exception("The Subject can not found or has been deleted!");
            }

            // Kiểm tra tính hợp lệ của StudentId và SlotId trước khi truy vấn
            if (tutorId == Guid.Empty)
            {
                throw new Exception("TutorId is invalid.");
            }
            if (subjectId == Guid.Empty)
            {
                throw new Exception("SubjectId is invalid.");
            }

            // Kiểm tra sự tồn tại và sự thay đổi của Tutor
            bool isChange = await _unitOfWork.GetRepository<TutorSubject>().Entities
                .AnyAsync(s =>
                    s.SubjectId == model.SubjectId &&
                    s.UserId == model.UserId);
            if (isChange)
            {
                throw new Exception("The Tutor does not have any changes.");
            }

            // Truy vấn để tìm schedule từ database dựa trên StudentId và SlotId
            TutorSubject existingTutorSubject = await _unitOfWork.GetRepository<TutorSubject>().Entities
                .FirstOrDefaultAsync(p => p.TutorId == tutorId && p.SubjectId == subjectId && !p.DeletedTime.HasValue)
                ?? throw new Exception("The Tutor cannot be found or deleted!");


            // Sử dụng AutoMapper để cập nhật dữ liệu từ model vào thực thể Schedule
            _mapper.Map(model, existingTutorSubject);

            // Thiết lập các thuộc tính bổ sung
            existingTutorSubject.LastUpdatedBy = "admin";
            existingTutorSubject.LastUpdatedTime = DateTimeOffset.UtcNow;

            _unitOfWork.TutorRepository.Update(existingTutorSubject);
            await _unitOfWork.SaveAsync();

            // Trả về thông tin đã cập nhật  
            return _mapper.Map<ResponseTutorModelViews>(existingTutorSubject);
        }

        public async Task<ResponseTutorModelViews> DeleteTutorSubjectAsync(Guid tutorId, Guid subjectId)
        {
            if (tutorId == Guid.Empty)
            {
                throw new Exception("Please provide a valid tutorId ID.");
            }
            if (subjectId == Guid.Empty)
            {
                throw new Exception("Please provide a valid subjectId ID.");
            }
            TutorSubject existingTutorSubject = await _unitOfWork.GetRepository<TutorSubject>().Entities
                .FirstOrDefaultAsync(p => p.TutorId == tutorId && p.SubjectId == subjectId && !p.DeletedTime.HasValue)
                ?? throw new Exception("The Tutor cannot be found or it has been deleted!");

            existingTutorSubject.DeletedTime = DateTimeOffset.UtcNow;
            existingTutorSubject.DeletedBy = "admin";
            _unitOfWork.TutorRepository.Update(existingTutorSubject);
            await _unitOfWork.SaveAsync();
            return _mapper.Map<ResponseTutorModelViews>(existingTutorSubject);
        }
    }
}