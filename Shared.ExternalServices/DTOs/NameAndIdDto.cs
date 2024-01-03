using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ExternalServices.DTOs
{
    public class NameAndIdDto<T>
    {
        public NameAndIdDto(string name, T id)
        {
            Name= name;
            Id = id;
        }
        public string Name { get; set; }
        public T Id { get; set; }
    }
}
