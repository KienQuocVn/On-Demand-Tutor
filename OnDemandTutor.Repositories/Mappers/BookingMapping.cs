using AutoMapper;
using OnDemandTutor.Contract.Repositories.Entity;
using OnDemandTutor.ModelViews.Booking;

namespace OnDemandTutor.Repositories.Mappers
{
    public class BookingMapping : Profile
    {
        public BookingMapping()
        {
            CreateMap<SlotBookingDto, Booking>();
            CreateMap<Booking, SlotBookingDto>();
        }
    }
}
