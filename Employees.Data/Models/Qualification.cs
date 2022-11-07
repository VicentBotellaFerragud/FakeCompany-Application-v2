using System;
using System.Collections.Generic;

namespace Employees.Data.Models
{
    public partial class Qualification
    {
        public Qualification()
        {
            EmployeeQualificationRefs = new HashSet<EmployeeQualificationRef>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<EmployeeQualificationRef> EmployeeQualificationRefs { get; set; }
    }
}
