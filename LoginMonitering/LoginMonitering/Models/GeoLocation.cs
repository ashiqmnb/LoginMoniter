using Microsoft.AspNetCore.Http.HttpResults;

namespace LoginMonitering.Models
{
    public class GeoLocation
    {
        public int Id { get; set; }
        public string Country { get; set; }
        public string RegionName { get; set; }
        public string City { get; set; }
        public string Timezone { get; set; }
        public string Query { get; set; }


        public LoginAttempt? LoginAttempt { get; set; }
    }
}
