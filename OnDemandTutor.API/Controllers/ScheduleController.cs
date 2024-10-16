using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnDemandTutor.Contract.Repositories.Entity;
using OnDemandTutor.Contract.Services.Interface;
using OnDemandTutor.Core.Base;
using OnDemandTutor.ModelViews.ScheduleModelViews;
using System;
using System.Threading.Tasks;

namespace OnDemandTutor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;

        public ScheduleController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        // GET: api/Schedule
        // lấy tất cả các lịch
        [HttpGet()]
        public async Task<ActionResult<BasePaginatedList<Schedule>>> GetAllSchedules(int pageNumber = 1, int pageSize = 5, Guid? studentId = null, Guid? slotId = null, string status = null)
        {
            try
            {
                var result = await _scheduleService.GetAllSchedulesAsync(pageNumber, pageSize, studentId, slotId, status);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // GET: api/Schedule
        // lấy tất cả lịch trừ những lịch đã được xóa
        [HttpGet("searchSchedule")]
        public async Task<ActionResult<BasePaginatedList<Schedule>>> SearchSchedules(int pageNumber = 1, int pageSize = 5, Guid? studentId = null, Guid? slotId = null, string status = null)
        {
            try
            {
                var result = await _scheduleService.GetAllSchedulesAsync(pageNumber, pageSize, studentId, slotId, status);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }


        // POST: api/Schedule
        // tạo 1 lịch mới chuyền vào studentID, SlotId, Status
        [HttpPost()]
        public async Task<ActionResult<Schedule>> CreateSchedule([FromBody] CreateScheduleModelViews model)
        {

            try
            {
                ResponseScheduleModelViews result = await _scheduleService.CreateScheduleAsync(model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // PUT: api/Schedule/{studentId}/{slotId}
        [HttpPut("{studentId}/{slotId}")]
        public async Task<IActionResult> UpdateSchedule(Guid studentId, Guid slotId, [FromBody] UpdateScheduleModelViews model)
        {
            try
            {
                // Gọi service để cập nhật schedule theo StudentId và SlotId
                ResponseScheduleModelViews result = await _scheduleService.UpdateScheduleAsync(studentId, slotId, model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // DELETE: api/Schedule/delete/{studentId}/{slotId}
        // Xóa lịch truyền vào gồm student id và slotId
        [HttpDelete("delete/{studentId}/{slotId}")]
        public async Task<IActionResult> DeleteSchedule(Guid studentId, Guid slotId)
        {
            try
            {
                ResponseScheduleModelViews result = await _scheduleService.DeleteScheduleAsync(studentId, slotId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

    }
}
