using System.Linq;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace InventarioAPI.Swagger
{
    public class ExamplesOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation == null || context == null) return;

            // Target the POST /api/pedidosclientes operation
            var apiDescriptor = context.ApiDescription;
            var relativePath = apiDescriptor.RelativePath?.ToLowerInvariant() ?? string.Empty;
            var httpMethod = apiDescriptor.HttpMethod?.ToUpperInvariant() ?? string.Empty;

            if (httpMethod == "POST" && relativePath.StartsWith("api/pedidosclientes"))
            {
                if (operation.RequestBody?.Content != null && operation.RequestBody.Content.ContainsKey("application/json"))
                {
                    var example = new OpenApiObject
                    {
                        ["clienteId"] = new OpenApiInteger(1),
                        ["detalles"] = new OpenApiArray
                        {
                            new OpenApiObject
                            {
                                ["productoId"] = new OpenApiInteger(10),
                                ["cantidad"] = new OpenApiInteger(2)
                            }
                        }
                    };

                    operation.RequestBody.Content["application/json"].Example = example;
                }
            }
        }
    }
}
