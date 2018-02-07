using Microsoft.AspNetCore.Http;
using System.IdentityModel.Services;
using System.Threading.Tasks;
using IdentityServer4.Hosting;

namespace IdentityServer4.WsFederation.Endpoints.Results
{
    public class SignInResult : IEndpointResult
    {
        public SignInResponseMessage Message { get; set; }

        public SignInResult(SignInResponseMessage message)
        {
            Message = message;
        }

        public Task ExecuteAsync(HttpContext context)
        {
            context.Response.ContentType = "text/html";
            return context.Response.WriteAsync(Message.WriteFormPost());
        }
    }
}