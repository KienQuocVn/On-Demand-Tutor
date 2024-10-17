using Microsoft.AspNetCore.Http;
using OnDemandTutor.ModelViews.AuthModelViews;
using OnDemandTutor.ModelViews.UserModelViews;

namespace OnDemandTutor.Contract.Services.Interface
{
    public interface IVNPayService
    {
        public string CreatePaymentUrl(HttpContext context, PaymentInfo model);
        public PaymentResponse PaymentExecute(IQueryCollection collections);
    }
}
