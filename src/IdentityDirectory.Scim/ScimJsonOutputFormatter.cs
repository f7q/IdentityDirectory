namespace IdentityDirectory.Scim
{
    using System.Buffers;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.Net.Http.Headers;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public class ScimJsonOutputFormatter : JsonOutputFormatter
    {
        public ScimJsonOutputFormatter(Newtonsoft.Json.JsonSerializerSettings json, ArrayPool<char> args): base(json, args)
        {

            this.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
			this.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
			this.SupportedMediaTypes.Clear();
			this.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(ScimConstants.ScimMediaType));
		}
	}
}