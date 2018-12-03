using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace OVE.Service.Core.Formatters {
    public class RawBodyInputFormatter : InputFormatter {
        public RawBodyInputFormatter() {
            this.SupportedMediaTypes.Add("application/json");
            this.SupportedMediaTypes.Add("application/xml");
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context) {
            var request = context.HttpContext.Request;
            using (var reader = new StreamReader(request.Body)) {
                var content = await reader.ReadToEndAsync();
                return await InputFormatterResult.SuccessAsync(content);
            }
        }

        protected override bool CanReadType(Type type) {
            return type == typeof(string);
        }
    }
}