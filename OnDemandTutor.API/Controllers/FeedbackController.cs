using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnDemandTutor.Contract.Repositories.Entity;
using OnDemandTutor.Contract.Services.Interface;
using OnDemandTutor.Core.Base;
using OnDemandTutor.ModelViews.FeedbackModelViews;
using OnDemandTutor.Services.Service;

namespace OnDemandTutor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private IFeedbackService _feedbackSevice;

        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackSevice = feedbackService;
        }



        [HttpGet("filter")]
        public async Task<ActionResult<Feedback>> GetFeedbackByFilter(int pageNumber, int pageSize, string? slotId, string? classId, Guid? studentId, Guid? tutorId, Guid? feedbackId)
        {
            try
            {
                // Gọi service để lấy feedback theo bộ lọc
                var feedback = await _feedbackSevice.GetFeedbackByFilterAsync(pageNumber, pageSize, slotId, classId, studentId, tutorId, feedbackId);

                // Trả về kết quả feedback
                return Ok(feedback);
            }
            catch (KeyNotFoundException ex)
            {
                // Trả về thông báo nếu không tìm thấy feedback
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                // Trả về lỗi nếu không cung cấp thông tin hợp lệ
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Trả về lỗi tổng quát nếu có lỗi khác
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lấy feedback.", details = ex.Message });
            }
        }
        [HttpGet("filler_delete")]
        public async Task<IActionResult> GetDeleteAtFeedbackAsync(int pageNumber, int pageSize, string? slotId, string? classId, Guid? studentId, Guid? tutorId, Guid? feedbackId)
        {
            try
            {
                // Gọi service để lấy feedback theo bộ lọc
                var feedback = await _feedbackSevice.GetDeleteAtFeedbackAsync(pageNumber, pageSize, slotId, classId, studentId, tutorId, feedbackId);

                // Trả về kết quả feedback
                return Ok(feedback);
            }
            catch (KeyNotFoundException ex)
            {
                // Trả về thông báo nếu không tìm thấy feedback
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                // Trả về lỗi nếu không cung cấp thông tin hợp lệ
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Trả về lỗi tổng quát nếu có lỗi khác
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lấy feedback.", details = ex.Message });
            }
        }

        [HttpPost()]
        public async Task<ActionResult<Feedback>> CreateFeedback(CreateFeedbackModelViews model)
        {
            try
            {
                var createdFeedback = await _feedbackSevice.CreateFeedbackAsync(model);
                return CreatedAtAction(nameof(GetFeedbackByFilter), new { id = createdFeedback.Id }, createdFeedback);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }


        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateFeedback(Guid id, Guid studentId, UpdateFeedbackModelViews model)
        {

            try
            {
                var result = await _feedbackSevice.UpdateFeedbackAsync(id, studentId, model);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteFeedback(Guid id, Guid studentId)
        {

            try
            {
                var result = await _feedbackSevice.DeleteFeedbackAsync(id, studentId);
                return result ? Ok(new { message = "Xóa thành công" }) : NotFound(new { message = "Không tìm thấy feedback để xóa." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
