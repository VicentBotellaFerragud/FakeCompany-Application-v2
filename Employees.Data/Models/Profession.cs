using System;
using System.Collections.Generic;

namespace Employees.Data.Models
{
    public partial class Profession
    {
        public Profession()
        {
            Employees = new HashSet<Employee>();
            InverseParentProfession = new HashSet<Profession>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int? ParentProfessionId { get; set; }

        public virtual Profession? ParentProfession { get; set; }
        public virtual ICollection<Employee> Employees { get; set; }
        public virtual ICollection<Profession> InverseParentProfession { get; set; }
    }
}
