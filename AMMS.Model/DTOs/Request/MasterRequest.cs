using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVEA.Model.DTOs.Request
{
    // DTO representing a master record
    public class MasterRequest
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Value { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
