using System.IdentityModel.Metadata;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Http;

namespace Rsk.IdentityServer4.WsFederation.Endpoints.Results
{
    public class MetadataResult : IEndpointResult
    {
        private readonly EntityDescriptor entity;

        public MetadataResult(EntityDescriptor entity)
        {
            this.entity = entity;
        }

        public Task ExecuteAsync(HttpContext context)
        {
            var ser = new MetadataSerializer();
            var sb = new StringBuilder(512);

            ser.WriteMetadata(XmlWriter.Create(new StringWriter(sb), new XmlWriterSettings { OmitXmlDeclaration = true }), entity);

            context.Response.ContentType = "application/xml";
            return context.Response.WriteAsync(sb.ToString());
        }
    }
}