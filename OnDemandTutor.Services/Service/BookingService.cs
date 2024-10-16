using OnDemandTutor.Contract.Repositories.Entity;
using OnDemandTutor.Contract.Repositories.Interface;
using OnDemandTutor.Contract.Repositories.IUOW;
using OnDemandTutor.Contract.Services.Interface;
using OnDemandTutor.ModelViews.Booking;
using OnDemandTutor.Repositories.Entity;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;


namespace OnDemandTutor.Services.Service

{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BookingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<string> BookSubjectBySlot(SlotBookingDto dto)
        {
            var student = await _unitOfWork.GetRepository<Accounts>().FindAsync(dto.StudentId);
            if (student == null)
                return "Student not found";

            var subject = await _unitOfWork.SubjectRepository.FindAsync(dto.SubjectId);
            if (subject == null)
                return "Subject not found";

            var tutorSubject = await _unitOfWork.TutorRepository.Entities
                .FirstOrDefaultAsync(ts => ts.TutorId == dto.TutorSubjectId);

            if (tutorSubject == null)
                return "Tutor for this subject not found";

            // Check if the selected slot is available  
            var selectedSlot = await _unitOfWork.SlotRepository.FindAsync(dto.SlotId);
            if (selectedSlot == null)
                return "Selected slot not found";

            // Check if the slot is already booked  
            var existingBookings = await _unitOfWork.GetRepository<Booking>().Entities
                .Where(b => b.SlotId == dto.SlotId)
                .ToListAsync();

            if (existingBookings.Any())
            {
                return "The selected slot is already booked.";
            }

            // Create a new Slot entry if needed  
            var newSlot = new Slot
            {
                ClassId = subject.ClassId, 
                StartTime = selectedSlot.StartTime,
                EndTime = selectedSlot.EndTime,
                Price = selectedSlot.Price,
                DayOfSlot = dto.SelectedDate.DayOfWeek.ToString()
            };

            await _unitOfWork.SlotRepository.InsertAsync(newSlot);
            await _unitOfWork.SaveAsync();

            // Create a new booking  
            var booking = new Booking
            {
                StudentId = dto.StudentId,
                SubjectId = dto.SubjectId,
                TutorId = tutorSubject.TutorId,
                SlotId = newSlot.Id,
                BookingDate = DateTime.UtcNow,
                TotalPrice = newSlot.Price // Assuming price is already set in the slot  
            };

            await _unitOfWork.GetRepository<Booking>().InsertAsync(booking);
            await _unitOfWork.SaveAsync();

            return $"Subject booked successfully. Total Price: {newSlot.Price:C}";
        }

        public async Task<string> BookSubjectByTime(TimeBookingDto dto)
        {
            var student = await _unitOfWork.GetRepository<Accounts>().FindAsync(dto.StudentId);
            if (student == null)
                return "Student not found";

            var subject = await _unitOfWork.SubjectRepository.FindAsync(dto.SubjectId);
            if (subject == null)
                return "Subject not found";

            var tutorSubject = await _unitOfWork.TutorRepository.Entities
                .FirstOrDefaultAsync(ts => ts.TutorId == dto.TutorSubjectId);

            if (tutorSubject == null)
                return "Tutor for this subject not found";

            // Check if the selected time is available  
            if (!await IsTimeAvailable(tutorSubject.TutorId, dto.SelectedDate, dto.StartTime, dto.EndTime))
            {
                return "The selected time is not available for this tutor.";
            }

            double durationInHours = (dto.EndTime - dto.StartTime).TotalHours;
            double totalPrice = CalculateTotalPrice(durationInHours); // Implement your pricing logic  

            var booking = new Booking
            {
                StudentId = dto.StudentId,
                SubjectId = dto.SubjectId,
                TutorId = tutorSubject.TutorId,
                BookingDate = DateTime.UtcNow,
                TotalPrice = totalPrice
            };

            await _unitOfWork.GetRepository<Booking>().InsertAsync(booking);
            await _unitOfWork.SaveAsync();

            return $"Subject booked successfully. Total Price: {totalPrice:C}";
        }

        // Phương thức kiểm tra khả dụng của ngày  
        private async Task<bool> IsTimeAvailable(Guid tutorId, DateTime selectedDate, TimeSpan startTime, TimeSpan endTime)
        {
            var existingBookings = await _unitOfWork.GetRepository<Booking>().Entities
                .Where(b => b.TutorId == tutorId && b.BookingDate.Date == selectedDate.Date)
                .ToListAsync();

            foreach (var booking in existingBookings)
            {
                // Assuming you have access to the slot times for each booking  
                if ((startTime < booking.EndTime) && (endTime > booking.StartTime))
                {
                    return false; // Time not available  
                }
            }

            return true; // Time is available  
        }

        private double CalculateTotalPrice(double durationInHours)
        {
            // Implement your pricing logic here  
            return durationInHours * 50; // Example pricing  
        }

    }
}
