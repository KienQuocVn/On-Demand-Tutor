using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnDemandTutor.Contract.Repositories.Entity;
using OnDemandTutor.Contract.Repositories.Interface;
using OnDemandTutor.Contract.Services.Interface;
using OnDemandTutor.Core.Base;
using OnDemandTutor.Core.Utils;
using OnDemandTutor.ModelViews.SubjectModelViews;
using OnDemandTutor.ModelViews.TutorSubjectModelViews;
using OnDemandTutor.Repositories.Entity;

namespace OnDemandTutor.Services.Service;

public class SubjectService : ISubjectService
{
    private readonly IUnitOfWork _unitOfWork;

    public SubjectService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BasePaginatedList<Subject>> GetAllSubject(int pageNumber, int pageSize)
    {
        // L?y danh s�ch m�n h?c ch?a b? x�a v� s?p x?p theo th?i gian t?o gi?m d?n  
        IQueryable<Subject> SubjectsQuery = _unitOfWork.GetRepository<Subject>()
            .Entities
            .Where(p => !p.DeletedTime.HasValue)
            .OrderByDescending(p => p.CreatedTime);

        // ??m t?ng s? m�n h?c kh�ng b? x�a  
        int totalCount = await SubjectsQuery.CountAsync();

        // L?y danh s�ch m�n h?c theo trang ?� ch? ??nh  
        var subjects = await SubjectsQuery
            .OrderBy(s => s.Id)  // S?p x?p theo Id  
            .Skip((pageNumber - 1) * pageSize) // B? qua s? l??ng m�n h?c c?a c�c trang tr??c  
            .Take(pageSize) // L?y s? l??ng m�n h?c theo k�ch th??c trang  
            .ToListAsync();

        // Tr? v? danh s�ch m�n h?c ?� ph�n trang  
        return new BasePaginatedList<Subject>(subjects, totalCount, pageNumber, pageSize);
    }

    public async Task<Subject> CreateSubjectAsync(CreateSubjectModelViews model)
    {
        // Ki?m tra t�nh h?p l? c?a m� h�nh  
        if (model == null || string.IsNullOrEmpty(model.Name))
        {
            Console.WriteLine("D? li?u m�n h?c kh�ng h?p l?.");
            throw new ArgumentException("D? li?u m�n h?c kh�ng h?p l?.");
        }

        try
        {
            // T?o ??i t??ng m�n h?c m?i t? m� h�nh  
            var subjectEntity = new Subject
            {
                Id = model.Id ?? Guid.NewGuid().ToString(), // T?o ID m?i n?u kh�ng c�  
                Name = model.Name,
                CreatedBy = model.CreatedBy,
                CreatedTime = DateTimeOffset.Now,
                LastUpdatedTime = DateTimeOffset.Now
            };

            // Th�m m�n h?c v�o c? s? d? li?u  
            await _unitOfWork.SubjectRepository.InsertAsync(subjectEntity);
            await _unitOfWork.SaveAsync(); // L?u thay ??i v�o c? s? d? li?u  

            // In ra th�ng b�o th�nh c�ng  
            Console.WriteLine($"T?o m?i m�n h?c th�nh c�ng: Id = {subjectEntity.Id}");

            // Tr? v? m�n h?c ?� t?o  
            return subjectEntity;
        }
        catch (Exception ex)
        {
            // In ra th�ng b�o l?i n?u c�  
            Console.WriteLine($"L?i khi t?o m?i m�n h?c: {ex.Message}");
            throw new Exception("Kh�ng th? t?o m?i m�n h?c", ex); // N�m ra l?i ?? x? l� ? n?i kh�c  
        }
    }


    public async Task<bool> DeleteSubject(string id)
    {
        // L?y m�n h?c hi?n t?i b?ng ID  
        var existingSubject = await _unitOfWork.GetRepository<Subject>().GetByIdAsync(id);

        // Ki?m tra xem m�n h?c c� t?n t?i kh�ng  
        if (existingSubject == null)
        {
            Console.WriteLine($"Kh�ng t�m th?y m�n h?c v?i ID: {id}");
            return false; // M�n h?c kh�ng t?n t?i  
        }

        // ?�nh d?u m�n h?c l� ?� x�a b?ng c�ch thi?t l?p DeletedTime v� DeletedBy  
        existingSubject.DeletedTime = CoreHelper.SystemTimeNow; // Thi?t l?p th?i gian x�a  
        existingSubject.DeletedBy = "admin"; // C� th? thay th? b?ng th�ng tin ng??i d�ng t? ng? c?nh  

        // C?p nh?t m�n h?c trong c? s? d? li?u  
        _unitOfWork.GetRepository<Subject>().Update(existingSubject);
        await _unitOfWork.SaveAsync(); // L?u thay ??i v�o c? s? d? li?u  

        // In ra th�ng b�o th�nh c�ng  
        Console.WriteLine($"M�n h?c v?i ID: {id} ?� ???c x�a th�nh c�ng.");
        return true;
    }


    public async Task<Subject> UpdateSubject(UpdateSubjectModel model)
    {
        // L?y m�n h?c hi?n t?i b?ng ID t? m� h�nh c?p nh?t  
        var existingSubject = await _unitOfWork.GetRepository<Subject>().GetByIdAsync(model.Id);

        // Ki?m tra xem m�n h?c c� t?n t?i kh�ng  
        if (existingSubject == null)
        {
            Console.WriteLine($"Kh�ng t�m th?y m�n h?c v?i ID: {model.Id}");
            return null; // M�n h?c kh�ng t?n t?i  
        }

        // C?p nh?t c�c thu?c t�nh c?a m�n h?c hi?n t?i  
        existingSubject.Name = model.Name; // C?p nh?t t�n m�n h?c  
        existingSubject.LastUpdatedBy = model.UpdateBy; // C?p nh?t ng??i th?c hi?n c?p nh?t  
        existingSubject.LastUpdatedTime = CoreHelper.SystemTimeNow; // C?p nh?t th?i gian c?p nh?t  

        // C?p nh?t m�n h?c trong c? s? d? li?u  
        _unitOfWork.GetRepository<Subject>().Update(existingSubject);
        await _unitOfWork.SaveAsync(); // L?u thay ??i v�o c? s? d? li?u  

        // In ra th�ng b�o th�nh c�ng  
        Console.WriteLine($"M�n h?c v?i ID: {model.Id} ?� ???c c?p nh?t th�nh c�ng.");
        return existingSubject; // Tr? v? m�n h?c ?� c?p nh?t  
    }


    public async Task<BasePaginatedList<Subject>> SearchSubjectsByNameAsync(string subjectName, int pageNumber, int pageSize)
    {
        // L?y danh s�ch Subject c� t�n ch?a subjectName v� ph�n trang  
        var query = _unitOfWork.SubjectRepository.Entities
            .Where(s => s.Name.Contains(subjectName));

        // L?y t?ng s? Subject th?a m�n ?i?u ki?n  
        var totalCount = await query.CountAsync();

        if (totalCount == 0)
        {
            return new BasePaginatedList<Subject>(new List<Subject>(), totalCount, pageNumber, pageSize);
        }

        // Th?c hi?n ph�n trang  
        var subjects = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new BasePaginatedList<Subject>(subjects, totalCount, pageNumber, pageSize);
    }
}