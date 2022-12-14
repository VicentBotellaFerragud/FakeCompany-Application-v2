using Employees.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Employees
{
    internal class Queries
    {
        private static EmployeesContext context = new();

        /*
            -- Which employees are working for a specific company?
            -- Which company/companies has/have the highest/lowest number of employees?
            -- Which employees are web developers?
            -- Which employees are software developers?
            -- Which employees are engineers?
            -- Which employees have the exact same qualifications as a given employee?
            -- Which employees have more than two qualifications in common?
            -- Which are the employees with the most unique qualifications?
         */

        public static List<Employee> GetEmployeesQuery()
        {
            var query =
                from e in context.Employees
                where e.ProfessionId == 1
                select e;

            List<Employee> employees = query.ToList();

            return employees;
        }

        public static List<Employee> GetEmployeesMethod()
        {
            List<Employee> employees =
                context.Employees.Where(e => e.ProfessionId == 1).ToList();

            return employees;
        }

        // -- Which employees are working for a specific company?

        public static List<Employee> GetEmployeesByCompanyQuery(int companyId)
        {
            var query =
                from e in context.Employees
                where e.CompanyId == companyId
                select e;

            List<Employee> employees = query.ToList();

            return employees;
        }

        public static List<Employee> GetEmployeesByCompanyMethod(int companyId)
        {
            List<Employee> employees =
                context.Employees.Where(e => e.CompanyId == companyId).ToList();

            return employees;
        }

        // -- Which company/companies has/have the highest/lowest number of employees?

        public static List<Company> GetCompaniesWithHighestNumberOfEmployeesQuery()
        {
            var firstQuery =
                from e in context.Employees
                where e.CompanyId != null
                group e by e.CompanyId into companyId
                select new
                {
                    CompanyId = companyId.Key,
                    NumberOfEmployees = companyId.Count()
                };

            int highestNumberOfEmployees =
                firstQuery.Max(c => c.NumberOfEmployees);

            var secondQuery =
                from c in firstQuery
                where c.NumberOfEmployees == highestNumberOfEmployees
                select c.CompanyId;

            var thirdQuery =
                from c in context.Companies
                where secondQuery.Contains(c.Id)
                select c;

            List<Company> finalList = thirdQuery.ToList();

            return finalList;
        }

        public static List<Company> GetCompaniesWithHighestNumberOfEmployeesMethod()
        {
            var companyIdsWithNumberOfEmployees =
                context.Employees
                .Where(e => e.CompanyId != null)
                .GroupBy(e => e.CompanyId)
                .Select(companyId => new { Id = companyId.Key, NumberOfEmployees = companyId.Count() });

            int highestNumberOfEmployees =
                companyIdsWithNumberOfEmployees.Max(c => c.NumberOfEmployees);

            var companyIdsWithHighestNumberOfEmployees =
                companyIdsWithNumberOfEmployees
                .Where(c => c.NumberOfEmployees == highestNumberOfEmployees)
                .Select(c => c.Id);

            List<Company> finalList =
                context.Companies.Where(c => companyIdsWithHighestNumberOfEmployees.Contains(c.Id)).ToList();

            return finalList;
        }

        // -- Which employees are web developers?

        public static List<Employee> GetAllWebDevelopersQuery()
        {
            var query =
                from e in context.Employees
                where e.Profession.Name == "Web Developer"
                select e;

            List<Employee> webDevelopers = query.ToList();

            return webDevelopers;
        }

        public static List<Employee> GetAllWebDevelopersMethod()
        {
            List<Employee> webDevelopers =
                context.Employees
                .Where(e => e.Profession.Name == "Web Developer")
                .ToList();

            return webDevelopers;
        }

        // -- Which employees are software developers?

        public static List<Employee> GetAllSoftwareDevelopersQuery()
        {
            var query =
                from e in context.Employees
                where e.Profession.Name == "Software Developer" || (e.Profession.ParentProfession != null && e.Profession.ParentProfession.Name == "Software Developer")
                select e;

            List<Employee> softwareDevelopers = query.ToList();

            return softwareDevelopers;
        }

        public static List<Employee> GetAllSoftwareDevelopersMethod()
        {
            List<Employee> softwareDevelopers =
                context.Employees
                .Where(e => e.Profession.Name == "Software Developer" || (e.Profession.ParentProfession != null && e.Profession.ParentProfession.Name == "Software Developer"))
                .ToList();

            return softwareDevelopers;
        }

        // -- Which employees are engineers?

        public static List<Employee> GetAllEngineersQuery() 
        {
            List<Profession> professionsThatAreRelated = new();

            int professionId = 1;

            var professionsTree = context.Professions.ToLookup(p => p.ParentProfessionId);

            //First node contains --> Professions with ParentProfessionId null.
            //Second node contains --> Professions with ParentProfessionId 1.
            //Third node contains --> Professions with ParentProfessionId 2.
            foreach (var node in professionsTree) 
            {
                if (node.Key == professionId) //Node key is "null" in the first iteration, "1" in the second and "2" in the third.
                {
                    professionsThatAreRelated.AddRange(professionsTree[node.Key]); //Adds S.Developer and M.Engineer in this case.
                }
                //Adds the children of any profession that has children and that is already in the "professionsThatAreRelated" list.
                else if (node.Key != null && professionsThatAreRelated.Select(p => p.Id).ToList().Contains((int)node.Key))
                {
                    professionsThatAreRelated.AddRange(professionsTree[node.Key]); //Adds Web, FS and DB Developers in this case.
                }
            }

            var engineers =
                from e in context.Employees
                where e.ProfessionId == professionId || professionsThatAreRelated.Select(p => p.Id).ToList().Contains(e.ProfessionId)
                select e;

            List<Employee> finalList = engineers.ToList();

            return finalList;
        }

        public static List<Employee> GetAllEngineersMethod()
        {
            int professionId = 1;

            List<Profession> professions = context.Professions.ToList();

            List<Profession> professionsThatAreRelated = professions.Where(p => p.Id == professionId).ToList();

            List<Profession> children = professions.Where(p => p.ParentProfessionId == professionId).ToList();  

            while (children.Any())            
            {
                professionsThatAreRelated.AddRange(children);
                children = professions.Where(p => children.Where(c => p.ParentProfessionId == c.Id).Any()).ToList();
            }

            List<Employee> engineers = 
                context.Employees
                .Where(e => professionsThatAreRelated.Select(p => p.Id).ToList().Contains(e.ProfessionId))
                .ToList();   

            return engineers;
        }

        // -- Bonus: Which employees are...?

        public static List<Employee> GetAllEmployeesOfACertainProfession(int professionId) //Sample case --> professionId = 1:
        {
            //List of all professions.
            List<Profession> lookup = context.Professions.ToList();

            //At first contains only the profession that corresponds to the passed-in professionId (Engineer).
            List<Profession> passedInProfessionPlusDescendants = lookup.Where(p => p.Id == professionId).ToList();

            //At first, list of all professions whose ParentProfessionId is the engineer id (S.Engineer and M.Engineer).
            List<Profession> children = lookup.Where(p => p.ParentProfessionId == professionId).ToList();

            //As long as the children list is not empty...
            while (children.Any())
            {
                //All children are added to the passedInProfessionPlusDescendants list.
                passedInProfessionPlusDescendants.AddRange(children);

                //A new empty list is created.
                List<Profession> childrensChildren = new();

                //For each profession in the children list... (At first, S.Engineer and M.Engineer)
                foreach (Profession c in children)
                {
                    //List of all professions whose ParentProfessionId is the S.Engineer id
                    //(in the 1st iteration, in the 2nd the list will contain all professions whose ParentProfessionId
                    //is the M.Engineer).
                    var cc = lookup.Where(p => p.ParentProfessionId == c.Id);

                    //Professions are added to the childrensChildren list.
                    childrensChildren.AddRange(cc);
                }

                //Finally the children list becomes the childrensChildren list.
                children = childrensChildren.ToList();

                //Because at some point the professions in the children list will have no more descendants (professions whose 
                //ParentProfessionIds are their Ids) the childrensChildren list will be empty after the foreach loop and, 
                //therefore, so will the children list.
            }

            List<Employee> allEmployeesOfACertainProfession =
                context.Employees
                .Where(e => passedInProfessionPlusDescendants.Select(p => p.Id).ToList().Contains(e.ProfessionId))
                .ToList();

            return allEmployeesOfACertainProfession;
        }

        // -- Bonus2: Which professions are the "descendants" from a given profession?

        public static List<Profession> GetAllDescendants(Profession profession, List<Profession> lookup)
        {
            var children = lookup.Where(p => p.ParentProfessionId == profession.Id);

            if (children.Any())
            {
                List<Profession> descendants = new();

                descendants.AddRange(children);

                foreach (var c in children)
                {
                    var d = GetAllDescendants(c, lookup);
                    descendants.AddRange(d);
                }

                return descendants;
            }
            else
            {
                return new List<Profession>();
            }
        }

        // -- Which employees have the exact same qualifications as a given employee?

        public static List<Employee> GetAllEmployeesThatHaveTheSameQualificationsAsAGivenEmployeeQuery(int givenEmployeeId)
        {
            var firstQuery =
                from eqr in context.EmployeeQualificationRefs
                where eqr.EmployeeId == givenEmployeeId 
                select eqr.QualificationId;

            var secondQuery =
                from e in context.Employees
                where e.EmployeeQualificationRefs.Select(eqr => eqr.QualificationId).All(element => firstQuery.Contains(element))
                where e.EmployeeQualificationRefs.Select(eqr => eqr.QualificationId).Distinct().Count() == firstQuery.Distinct().Count()
                where e.Id != givenEmployeeId
                select e;

            List<Employee> employeesWithTheSameQualificationsAsTheGivenEmployee = secondQuery.ToList();

            return employeesWithTheSameQualificationsAsTheGivenEmployee;
        }

        public static List<Employee> GetAllEmployeesThatHaveTheSameQualificationsAsAGivenEmployeeMethod(int givenEmployeeId)
        {
            var givenEmployeeQualificationIds =
                context.EmployeeQualificationRefs
                .Where(q => q.EmployeeId == givenEmployeeId)
                .Select(q => q.QualificationId);

            List<Employee> employeesWithTheSameQualificationsAsTheGivenEmployee =
                context.Employees
                .Where(e => e.EmployeeQualificationRefs.Select(eqr => eqr.QualificationId).All(element => givenEmployeeQualificationIds.Contains(element)))
                .Where(e => e.EmployeeQualificationRefs.Select(eqr => eqr.QualificationId).Distinct().Count() == givenEmployeeQualificationIds.Distinct().Count())
                .Where(e => e.Id != givenEmployeeId)
                .Select(e => e)
                .ToList();

            return employeesWithTheSameQualificationsAsTheGivenEmployee;
        }

        // -- Which employees have more than two qualifications in common?

        public static List<Employee> GetAllEmployeesThatHaveMoreThanTwoQualificationsInCommon()
        {
            int iterationsCount = 0;

            var employees =
                context.Employees
                .Include(e => e.EmployeeQualificationRefs)
                .Where(e => e.EmployeeQualificationRefs.Select(eqr => eqr.QualificationId).Distinct().Count() > 2) 
                .Select(e => e)
                .ToList();

            List<Employee> employeesWithMoreThanTwoQualificationsInCommon = new();
            
            foreach (var employee in employees)
            {
                iterationsCount++;

                List<int> employeeQualifications = employee.EmployeeQualificationRefs.Select(eqr => eqr.QualificationId).Distinct().ToList();

                foreach (var colleague in employees.Where(e => e.Id > employee.Id))
                {
                    iterationsCount++;

                    List<int> colleagueQualifications = colleague.EmployeeQualificationRefs.Select(eqr => eqr.QualificationId).Distinct().ToList();

                    int count = colleagueQualifications.Where(qId => employeeQualifications.Contains(qId)).ToList().Count;

                    if (count > 2)
                    {
                        employeesWithMoreThanTwoQualificationsInCommon.AddRange(new List<Employee> { employee, colleague });
                    }
                }
            }

            List<Employee> employeesWithMoreThanTwoQualificationsInCommonFiltered =
                employeesWithMoreThanTwoQualificationsInCommon
                .Select(e => e)
                .Distinct()
                .ToList();

            return employeesWithMoreThanTwoQualificationsInCommonFiltered;
        }

        public static List<Employee> GetAllEmployeesThatHaveMoreThanTwoQualificationsInCommon2()
        {
            int iterationsCount = 0;

            var employees =
                context.Employees
                .Include(e => e.EmployeeQualificationRefs)
                .Where(e => e.EmployeeQualificationRefs.Select(eqr => eqr.QualificationId).Distinct().Count() > 2)
                .Select(e => e)
                .ToList();

            List<Employee> employeesWithMoreThanTwoQualificationsInCommon = new();

            while (employees.Any())
            {
                iterationsCount++;

                Employee employee = employees.First();

                List<int> employeeQualifications = employee.EmployeeQualificationRefs.Select(eqr => eqr.QualificationId).Distinct().ToList();

                List<Employee> colleagues = new(employees.Where(e => e.Id > employee.Id));

                List<Employee> colleaguesWithMoreThanTwoQualificationsInCommon = new();

                while (colleagues.Any())
                {
                    iterationsCount++;

                    Employee colleague = colleagues.First();

                    List<int> colleagueQualifications = colleague.EmployeeQualificationRefs.Select(eqr => eqr.QualificationId).Distinct().ToList();

                    int numberOfQualificationsInCommon = colleagueQualifications.Where(qId => employeeQualifications.Contains(qId)).ToList().Count;

                    if (numberOfQualificationsInCommon > 2)
                    {
                        colleaguesWithMoreThanTwoQualificationsInCommon.Add(colleague);     
                    }

                    colleagues.Remove(colleague);                 
                }

                if (colleaguesWithMoreThanTwoQualificationsInCommon.Any())
                {
                    employeesWithMoreThanTwoQualificationsInCommon.Add(employee);
                    employeesWithMoreThanTwoQualificationsInCommon.AddRange(colleaguesWithMoreThanTwoQualificationsInCommon);
                }

                employees.Remove(employee);
            }

            List<Employee> employeesWithMoreThanTwoQualificationsInCommonFiltered =
                employeesWithMoreThanTwoQualificationsInCommon
                .Select(e => e)
                .Distinct()
                .ToList();

            return employeesWithMoreThanTwoQualificationsInCommonFiltered;
        }

        // -- Which are the employees with the most unique qualifications?

        public static List<Employee> GetAllEmployeesWithTheMostUniqueQualifications()
        {
            int iterationsCount = 0;

            List<Employee> employees =
                context.Employees
                .Include(e => e.EmployeeQualificationRefs)
                .Select(e => e)
                .ToList();

            List<Employee> masters = new();

            foreach (var e1 in employees)
            {
                iterationsCount++;

                bool isE1Master = true;

                List<int> q1 = e1.EmployeeQualificationRefs.Select(eqr => eqr.QualificationId).Distinct().ToList();

                foreach (var e2 in employees)
                {
                    iterationsCount++;

                    List<int> q2 = e2.EmployeeQualificationRefs.Select(eqr => eqr.QualificationId).Distinct().ToList();

                    bool hasE2AllQualificationsFromE1 = q1.All(q => q2.Contains(q));

                    bool hasE2MoreQualificationsThanE1 = q2.Count > q1.Count;

                    bool isE2MoreQualifiedThanE1 = hasE2AllQualificationsFromE1 && hasE2MoreQualificationsThanE1;

                    if (isE2MoreQualifiedThanE1)
                    {
                        isE1Master = false;
                        break;
                    }
                }

                if (isE1Master)
                {
                    masters.Add(e1);
                }
            }

            return masters;
        }

        public static List<Employee> GetAllEmployeesWithTheMostUniqueQualifications2()
        {
            List<Employee> employees =
                context.Employees
                .Include(e => e.EmployeeQualificationRefs)
                .Select(e => e)
                .ToList();

            List<Employee> masters = new();

            foreach (var e in employees)
            {
                List<int> eQualifications = e.EmployeeQualificationRefs.Select(eqr => eqr.QualificationId).Distinct().ToList();

                List<Employee> employeesThatAreMoreQualified =
                     employees
                     .Where(e => eQualifications.All(q => e.EmployeeQualificationRefs.Select(eqr => eqr.QualificationId).Distinct().Contains(q)))
                     .Where(e => e.EmployeeQualificationRefs.Select(eqr => eqr.QualificationId).Distinct().Count() > eQualifications.Count)
                     .Select(e => e)
                     .ToList();

                if (!employeesThatAreMoreQualified.Any())
                {
                    masters.Add(e);
                }
            }

            return masters;
        }
    }
}
