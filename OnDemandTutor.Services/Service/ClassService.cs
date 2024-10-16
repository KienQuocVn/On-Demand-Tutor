using OnDemandTutor.Contract.Repositories.Interface;
using OnDemandTutor.Contract.Services.Interface;


using AutoMapper;
using OnDemandTutor.ModelViews.ClassModelViews;
using OnDemandTutor.Core.Base;
using OnDemandTutor.Contract.Repositories.Entity;
using Microsoft.EntityFrameworkCore;
using OnDemandTutor.Repositories.Entity;

namespace OnDemandTutor.Services.Service
{
    public class ClassService : IClassService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ClassService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;

        }

        // Lấy về tất cả các class chưa bị xóa mềm và lọc theo các tham số được truyền vào nếu có
        public async Task<BasePaginatedList<Class>> GetAllClassesAsync(int pageNumber, int pageSize, Guid? classId, Guid? accountId, Guid? subjectId, DateTime? startDay, DateTime? endDay)
        {
            // Lấy tất cả các bản ghi từ bảng Class với điều kiện tìm kiếm
            IQueryable<Class> classesQuery = _unitOfWork.GetRepository<Class>().Entities
                .Where(p => !p.DeletedTime.HasValue || string.IsNullOrEmpty(p.DeletedBy))
                .OrderByDescending(c => c.CreatedTime);

            // Điều kiện tìm kiếm theo classId nếu có
            if (classId.HasValue)
            {
                classesQuery = classesQuery.Where(c => c.Id == classId);
            }

            // Điều kiện tìm kiếm theo accountId nếu có
            if (accountId.HasValue)
            {
                classesQuery = classesQuery.Where(c => c.AccountId == accountId);
            }

            // Điều kiện tìm kiếm theo subjectId nếu có
            if (subjectId.HasValue)
            {
                classesQuery = classesQuery.Where(c => c.SubjectId == subjectId);
            }

            // Điều kiện lọc theo ngày bắt đầu nếu có
            if (startDay.HasValue)
            {
                classesQuery = classesQuery.Where(c => c.StartDay >= startDay.Value);
            }

            // Điều kiện lọc theo ngày kết thúc nếu có
            if (endDay.HasValue)
            {
                classesQuery = classesQuery.Where(c => c.EndDay <= endDay.Value);
            }

            // Đếm tổng số bản ghi phù hợp với điều kiện tìm kiếm
            int totalCount = await classesQuery.CountAsync();

            // Áp dụng phân trang nếu không tìm kiếm theo classId
            if (classId == Guid.Empty)
            {
                classesQuery = classesQuery
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize);
            }

            List<Class> paginatedClasses = await classesQuery.ToListAsync();

            return new BasePaginatedList<Class>(paginatedClasses, totalCount, pageNumber, pageSize);
        }

        // tạo 1 class mới với tham số chuyền vào AccountId SubjectId AmountOfSlot StartDay EndDay 
        public async Task<ResponseClassModelView> CreateClassAsync(CreateClassModelView model)
        {
            // Kiểm tra các thuộc tính không được để trống hoặc giá trị hợp lệ
            if (model.AccountId == Guid.Empty)
            {
                throw new Exception("Please enter a valid AccountId.");
            }

            if (model.SubjectId == Guid.Empty)
            {
                throw new Exception("Please enter a valid SubjectId.");
            }

            if (model.AmountOfSlot <= 0)
            {
                throw new Exception("The amount of slots must be greater than zero.");
            }

            if (model.StartDay == default)
            {
                throw new Exception("Please enter a valid StartDay.");
            }

            if (model.EndDay == default)
            {
                throw new Exception("Please enter a valid EndDay.");
            }

            if (model.StartDay >= model.EndDay)
            {
                throw new Exception("StartDay must be earlier than EndDay.");
            }

            // Kiểm tra sự tồn tại của Account
            bool isExistAccount = await _unitOfWork.GetRepository<Accounts>().Entities
                .AnyAsync(a => a.Id == model.AccountId && !a.DeletedTime.HasValue);

            if (!isExistAccount)
            {
                throw new Exception("The Account cannot be found or has been deleted!");
            }

            // Kiểm tra sự tồn tại của Subject
            //bool isExistSubject = await _unitOfWork.GetRepository<Subject>().Entities
            //    .AnyAsync(s => s.Id == model.SubjectId && !s.DeletedTime.HasValue);

            //if (!isExistSubject)
            //{
            //    throw new Exception("The Subject cannot be found or has been deleted!");
            //}


            Class newClass = _mapper.Map<Class>(model);

            // Thiết lập thêm các thuộc tính không có trong CreateClassModelView
            newClass.Id = Guid.NewGuid();
            newClass.CreatedBy = "claim account";  // Ví dụ: lấy từ thông tin xác thực
            newClass.CreatedTime = DateTimeOffset.UtcNow;
            newClass.LastUpdatedTime = DateTimeOffset.UtcNow;

            await _unitOfWork.ClassRepository.InsertAsync(newClass);
            await _unitOfWork.SaveAsync();

            return _mapper.Map<ResponseClassModelView>(newClass);
        }


        // cập nhật thông tin của class
        public async Task<ResponseClassModelView> UpdateClassAsync(Guid id, UpdateClassModelView model)
        {
            if (id == Guid.Empty)
            {
                throw new Exception("Please enter a Class ID can update.");
            }

            // Kiểm tra sự tồn tại của Class
            Class existingClass = await _unitOfWork.GetRepository<Class>().Entities
                .FirstOrDefaultAsync(c => c.Id == id && !c.DeletedTime.HasValue)
                ?? throw new Exception("The Class cannot be found or has been deleted!");

            // Kiểm tra sự tồn tại của Account
            bool isExistAccount = await _unitOfWork.GetRepository<Accounts>().Entities
                .AnyAsync(a => a.Id == model.AccountId && !a.DeletedTime.HasValue);

            if (!isExistAccount)
            {
                throw new Exception("The Account cannot be found or has been deleted!");
            }

            // Kiểm tra sự tồn tại của Subject
            //bool isExistSubject = await _unitOfWork.GetRepository<Subject>().Entities
            //    .AnyAsync(s => s.Id == model.SubjectId && !s.DeletedTime.HasValue);

            //if (!isExistSubject)
            //{
            //    throw new Exception("The Subject cannot be found or has been deleted!");
            //}

            // Kiểm tra tính hợp lệ của AmountOfSlot
            if (model.AmountOfSlot <= 0)
            {
                throw new Exception("AmountOfSlot must be greater than 0.");
            }

            // Kiểm tra ngày bắt đầu
            if (model.StartDay == default)
            {
                throw new Exception("StartDay cannot be empty.");
            }

            // Kiểm tra xem ngày bắt đầu có lớn hơn ngày hiện tại không
            if (model.StartDay < DateTime.UtcNow)
            {
                throw new Exception("StartDay must be greater than the current date.");
            }

            // Kiểm tra ngày kết thúc
            if (model.EndDay == default)
            {
                throw new Exception("EndDay cannot be empty.");
            }

            // Kiểm tra tính hợp lệ của ngày bắt đầu và ngày kết thúc
            if (model.StartDay >= model.EndDay)
            {
                throw new Exception("StartDay must be earlier than EndDay.");
            }

            // Kiểm tra sự thay đổi trong thực thể
            if (existingClass.AccountId == model.AccountId &&
                existingClass.SubjectId == model.SubjectId &&
                existingClass.AmountOfSlot == model.AmountOfSlot &&
                existingClass.StartDay == model.StartDay &&
                existingClass.EndDay == model.EndDay)
            {
                throw new Exception("No changes detected to update.");
            }

            // Sử dụng AutoMapper để ánh xạ từ model sang thực thể Class
            _mapper.Map(model, existingClass);

            // Thiết lập các thuộc tính bổ sung
            existingClass.LastUpdatedBy = "claim account";  // Có thể lấy từ thông tin xác thực người dùng
            existingClass.LastUpdatedTime = DateTimeOffset.UtcNow;

            // Cập nhật thực thể Class vào database
            _unitOfWork.ClassRepository.Update(existingClass);
            await _unitOfWork.SaveAsync();

            // Trả về kết quả sau khi cập nhật
            return _mapper.Map<ResponseClassModelView>(existingClass);
        }

        public async Task<ResponseClassModelView> DeleteClassAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new Exception("Please enter a valid Class ID.");
            }

            // Lấy Class từ database dựa trên ID
            Class existingClass = await _unitOfWork.GetRepository<Class>().Entities
                .FirstOrDefaultAsync(c => c.Id == id && !c.DeletedTime.HasValue)
                ?? throw new Exception("The Class cannot be found or has been deleted!");

            // Thực hiện xóa mềm
            existingClass.DeletedTime = DateTimeOffset.UtcNow;
            existingClass.DeletedBy = "claim account";  // Có thể thay thế bằng thông tin người dùng đăng nhập

            // Cập nhật thực thể Class trong cơ sở dữ liệu
            _unitOfWork.ClassRepository.Update(existingClass);
            await _unitOfWork.SaveAsync();

            // Trả về đối tượng đã được xóa sau khi đã map sang ResponseClassModelView
            return _mapper.Map<ResponseClassModelView>(existingClass);
        }

        public async Task<double> CalculateTotalAmount(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new Exception("Please enter a valid Class ID.");
            }

            Class existedClass = await _unitOfWork.GetRepository<Class>().Entities
                .FirstOrDefaultAsync(c => c.Id == id && !c.DeletedTime.HasValue)
                ?? throw new Exception("The Class cannot be found or has been deleted!");

            Slot existedSlot = await _unitOfWork.GetRepository<Slot>().Entities
                .FirstOrDefaultAsync(c => c.ClassId == existedClass.Id && !c.DeletedTime.HasValue)
                ?? throw new Exception("The Slot cannot be found or has been deleted!");

            return existedClass.AmountOfSlot * existedSlot.Price;
        }
    }
}
