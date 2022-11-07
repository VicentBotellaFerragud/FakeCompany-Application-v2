using System;
using System.Collections.Generic;

namespace Employees.Data.Models
{
    public partial class Employee
    {
        public Employee()
        {
            EmployeeQualificationRefs = new HashSet<EmployeeQualificationRef>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int? CompanyId { get; set; }
        public int ProfessionId { get; set; }

        public virtual Company? Company { get; set; }
        public virtual Profession Profession { get; set; } = null!;
        public virtual ICollection<EmployeeQualificationRef> EmployeeQualificationRefs { get; set; }
    }
}
