using System.Collections.Generic;

namespace MobileFlat.Dto
{
    public class AccountDto
    {
        public bool Success { get; set; }
        public List<AccountDataDto> Data { get; set; }
    }
}
