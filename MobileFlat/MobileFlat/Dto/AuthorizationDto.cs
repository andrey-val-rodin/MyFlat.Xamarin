using System.Collections.Generic;

namespace MobileFlat.Dto
{
    public class AuthorizationDto
    {
        public bool Success { get; set; }
        public List<AuthorizationDataDto> Data { get; set; }
    }
}
