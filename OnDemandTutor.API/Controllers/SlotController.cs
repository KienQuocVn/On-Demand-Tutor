using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnDemandTutor.Contract.Repositories.Entity;
using OnDemandTutor.Contract.Services.Interface;
using OnDemandTutor.ModelViews.SLotModelViews;
using OnDemandTutor.Services.Service;

namespace OnDemandTutor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SlotController : ControllerBase
    {
        private ISlotSevice _slotService;

        public SlotController(ISlotSevice slotSevice) { 
            _slotService = slotSevice;
        }

        [HttpGet("filter")]
        public async Task<ActionResult<Slot>> GetAllSlotByFilter(int pageNumber, int pageSize, Guid? id, Guid? classId, TimeSpan? StartTime, TimeSpan? endTime, double? price)
        {
            try
            {
                // Gọi service để lấy slot theo bộ lọc
                var slot = await _slotService.GetAllSlotByFilterAsync(pageNumber, pageSize, id, classId, StartTime, endTime, price);

                // Trả về kết quả slot
                return Ok(slot);
            }
            catch (KeyNotFoundException ex)
            {
                // Trả về thông báo nếu không tìm thấy slot
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
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lấy slot.", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<Slot>> CreateSlot(SlotModelView model)
        {
            try
            {
                var createdSlot = await _slotService.CreateSlotAsync(model);
                return CreatedAtAction(nameof(GetAllSlotByFilter), new { id = createdSlot.Id }, createdSlot);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateSlot(Guid id, SlotModelView model)
        {
            try
            {
                var result = await _slotService.UpdateSlotAsync(id, model);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi update slot", error = ex.Message });
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteSlot(Guid id)
        {

            try
            {
                var result = await _slotService.DeleteSlotAsync(id);
                return result ? Ok(new { message = "Xóa thành công" }) : NotFound(new { message = "Không tìm thấy feedback để xóa." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
           
}
