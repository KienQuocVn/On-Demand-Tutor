using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnDemandTutor.Contract.Repositories.Entity;
using OnDemandTutor.Contract.Services.Interface;
using OnDemandTutor.Core.Base;
using OnDemandTutor.ModelViews.SubjectModelViews;
using OnDemandTutor.Services.Service;

namespace OnDemandTutor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubjectController : ControllerBase
    {
        private readonly ISubjectService _subjectService;
        public SubjectController(ISubjectService subjectService) => _subjectService = subjectService;

        [HttpGet]
        public async Task<ActionResult<BasePaginatedList<Subject>>> GetAllSubjects(int pageNumber, int pageSize)
        {
            try
            {
                var subjects = await _subjectService.GetAllSubject(pageNumber, pageSize);
                return Ok(BaseResponse<BasePaginatedList<Subject>>.OkResponse(subjects));
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message }); // Tr? v? th�ng b�o l?i  
            }
        }

        // Ph??ng th?c POST ?? th�m m?i Subject  
        [HttpPost("new_subject")]
        public async Task<IActionResult> AddSubject([FromBody] CreateSubjectModelViews model)
        {
            try
            {
                var subject = await _subjectService.CreateSubjectAsync(model);
                return Ok(BaseResponse<Subject>.OkResponse(subject)); // Tr? v? Subject ?� t?o  
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message }); // Tr? v? th�ng b�o l?i  
            }
        }

        // Ph??ng th?c c?p nh?t m�n h?c  
        [HttpPut("update_subject")]
        public async Task<IActionResult> UpdateSubject([FromBody] UpdateSubjectModel model)
        {
            // G?i d?ch v? ?? c?p nh?t m�n h?c  
            var updatedSubject = await _subjectService.UpdateSubject(model);

            // Ki?m tra k?t qu? c?p nh?t  
            if (updatedSubject == null)
            {
                return BadRequest(new { Message = "Kh�ng t�m th?y m�n h?c v?i ID: " + model.Id });
            }

            // Tr? v? th�ng b�o th�nh c�ng  
            return Ok(new { Message = "C?p nh?t m�n h?c th�nh c�ng", Subject = updatedSubject });
        }

        // Ph??ng th?c x�a m�n h?c  
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubject(string id)
        {
            // G?i d?ch v? ?? x�a m�n h?c  
            var result = await _subjectService.DeleteSubject(id);

            // Ki?m tra k?t qu? x�a  
            if (!result)
            {
                return BadRequest(new { Message = "Kh�ng t�m th?y m�n h?c v?i ID: " + id });
            }

            // Tr? v? m� 204 No Content n?u x�a th�nh c�ng  
            return NoContent();
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchSubjectsByName(string subjectName, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var result = await _subjectService.SearchSubjectsByNameAsync(subjectName, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "C� l?i x?y ra khi t�m ki?m m�n h?c.");
            }
        }
    }
}
