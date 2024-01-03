using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.ExternalServices.DTOs;

namespace Shared.ExternalServices.ViewModels.Response
{
    public record AvailableSapProductResponse(List<NameAndIdDto<string>> SapProducts);
}
