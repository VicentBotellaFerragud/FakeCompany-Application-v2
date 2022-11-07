using System;
using System.Collections.Generic;

namespace Employees.Data.Models
{
    public partial class EmployeeQualificationRef
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int QualificationId { get; set; }

        public virtual Employee Employee { get; set; } = null!;
        public virtual Qualification Qualification { get; set; } = null!;
    }
}
