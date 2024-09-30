using OnDemandTutor.Contract.Repositories.Entity;
using OnDemandTutor.Core.Base;
using OnDemandTutor.ModelViews.SubjectModelViews;

namespace OnDemandTutor.Contract.Services.Interface;

// ??nh ngh?a m?t giao di?n cho d?ch v? qu?n l� c�c ??i t??ng Subject  
public interface ISubjectService
{
    // Ph??ng th?c ?? l?y t?t c? c�c Subject v?i ph�n trang  
    // pageNumber: s? trang (th? t? t? 1)  
    // pageSize: s? l??ng Subject tr�n m?i trang  
    // Tr? v? m?t danh s�ch c�c Subject ???c ph�n trang  
    Task<BasePaginatedList<Subject>> GetAllSubject(int pageNumber, int pageSize);

    // Ph??ng th?c ?? t?o m?i m?t Subject  
    // model: ??i t??ng ch?a th�ng tin c?n thi?t ?? t?o Subject  
    // Tr? v? Subject m?i ?� ???c t?o  
    Task<Subject> CreateSubjectAsync(CreateSubjectModelViews model); // C?p nh?t ph??ng th?c  

    // Ph??ng th?c ?? x�a m?t Subject d?a tr�n ID  
    // id: ??nh danh c?a Subject c?n x�a  
    // Tr? v? true n?u x�a th�nh c�ng, false n?u kh�ng  
    Task<bool> DeleteSubject(string id);

    // Ph??ng th?c ?? c?p nh?t th�ng tin c?a m?t Subject  
    // model: ??i t??ng ch?a th�ng tin c?n c?p nh?t  
    // Tr? v? Subject ?� ???c c?p nh?t  
    Task<Subject> UpdateSubject(UpdateSubjectModel model);


    Task<BasePaginatedList<Subject>> SearchSubjectsByNameAsync(string subjectName, int pageNumber, int pageSize);
}