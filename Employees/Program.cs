using Employees.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Employees
{
    public class Program
    {
        private static EmployeesContext context = new EmployeesContext();

        private static void Main(string[] args)
        {
            //GetCompanies();
            //CreateEmployees();
            //AssignQualifications(1, 4);
            //AssignQualificationsBugged(1, 4);
            //EmptyTable("EmployeeQualificationRef");

            /* Queries.cs methods */

            //Queries.GetEmployeesQuery();
            //Queries.GetEmployeesMethod();

            //Queries.GetEmployeesByCompanyQuery(4);
            //Queries.GetEmployeesByCompanyMethod(3);

            //Queries.GetCompaniesWithHighestNumberOfEmployeesQuery();
            //Queries.GetCompaniesWithHighestNumberOfEmployeesMethod();

            //Queries.GetAllWebDevelopersQuery();
            //Queries.GetAllWebDevelopersMethod();

            //Queries.GetAllSoftwareDevelopersQuery();
            //Queries.GetAllSoftwareDevelopersMethod();

            //Queries.GetAllEngineersMethod();
            //Queries.GetAllEngineersMethod2();

            //Queries.GetAllEmployeesThatHaveTheSameQualificationsAsAGivenEmployeeQuery(25);
            //Queries.GetAllEmployeesThatHaveTheSameQualificationsAsAGivenEmployeeMethod(25);

            //Queries.GetAllEmployeesThatHaveMoreThanTwoQualificationsInCommonQuery();
            //Queries.GetAllEmployeesThatHaveMoreThanTwoQualificationsInCommonMethod();

            Console.ReadKey();
        }

        private static void EmptyTable(string table)
        {
            using EmployeesContext db = new EmployeesContext();

            /*
            //Table structure, attributes, and indexes remain intact.
            db.Database.ExecuteSqlRaw($"DELETE FROM {table}");
            */

            //Resets a particular table's identity
            db.Database.ExecuteSqlRaw($"TRUNCATE TABLE {table}");
        }

        private static void GetCompanies()
        {
            List<Company> companies = context.Companies.ToList();

            Console.WriteLine("Companies from the database:");
            Console.WriteLine("");
            foreach (var company in companies)
            {
                Console.WriteLine(company.Name);
                Console.WriteLine("");
            }
        }

        private static void CreateEmployees()
        {
            using EmployeesContext db = new EmployeesContext();

            List<Employee> employees = new List<Employee>();

            string filePath = @"names.txt";
            List<string> names = File.ReadAllLines(filePath).ToList();

            Random rnd = new Random();

            /* For the CompanyId */

            List<int?> companyIds = db.Companies.Select(c => (int?)c.Id).ToList();
            companyIds.Add(null);

            /* For the ProfessionId */

            List<int> professionIds = db.Professions.Select(p => p.Id).ToList();

            foreach (string name in names)
            {
                int randomCompanyIndex = rnd.Next(0, companyIds.Count);
                int randomProfessionIndex = rnd.Next(0, professionIds.Count);

                employees.Add(
                    new Employee 
                    {
                        Name = name,
                        CompanyId = companyIds[randomCompanyIndex],
                        ProfessionId = professionIds[randomProfessionIndex] 
                    });
            }

            db.Employees.AddRange(employees);
            db.SaveChanges();   

            foreach (Employee employee in employees)
            {
                Console.WriteLine(employee.Name);
                Console.WriteLine(employee.CompanyId);
                Console.WriteLine(employee.ProfessionId);
                Console.WriteLine("");
            }
        }

        private static void AssignQualifications(int min, int max)
        {
            using EmployeesContext db = new EmployeesContext();

            List<EmployeeQualificationRef> employeeQualificationRefs = new List<EmployeeQualificationRef>();
            List<int> employeeIds = db.Employees.Select(e => e.Id).ToList();
            List<int> qualificationIds = db.Qualifications.Select(q => q.Id).ToList();

            Random rnd = new Random();

            foreach (int employeeId in employeeIds)
            {
                List<int> availableQualificationIds = new List<int>(qualificationIds);
                int randomNumberOfQualifications = rnd.Next(min, max + 1);

                for (int i = 1; i <= randomNumberOfQualifications; i++)
                {
                    int randomQualificationIndex = rnd.Next(0, availableQualificationIds.Count);

                    employeeQualificationRefs.Add(
                        new EmployeeQualificationRef
                        {
                            EmployeeId = employeeId,
                            QualificationId = availableQualificationIds[randomQualificationIndex]
                        });

                    availableQualificationIds.RemoveAt(randomQualificationIndex);   
                }
            }

            db.EmployeeQualificationRefs.AddRange(employeeQualificationRefs);
            db.SaveChanges();

            foreach (EmployeeQualificationRef employeeQualificationRef in employeeQualificationRefs)
            {
                Console.WriteLine(employeeQualificationRef.EmployeeId);
                Console.WriteLine(employeeQualificationRef.QualificationId);
                Console.WriteLine("");
            }
        }

        private static void AssignQualificationsBugged(int min, int max)
        {
            using EmployeesContext db = new EmployeesContext();

            List<EmployeeQualificationRef> employeeQualificationRefs = new List<EmployeeQualificationRef>();
            List<int> employeeIds = db.Employees.Select(e => e.Id).ToList();
            List<int> qualificationIds = db.Qualifications.Select(q => q.Id).ToList();

            Random rnd = new Random();

            foreach (int employeeId in employeeIds)
            {
                int randomNumberOfQualifications = rnd.Next(min, max + 1);

                for (int i = 1; i <= randomNumberOfQualifications; i++)
                {
                    int randomQualificationIndex = rnd.Next(0, qualificationIds.Count);

                    employeeQualificationRefs.Add(
                        new EmployeeQualificationRef
                        {
                            EmployeeId = employeeId,
                            QualificationId = qualificationIds[randomQualificationIndex]
                        });
                }
            }

            db.EmployeeQualificationRefs.AddRange(employeeQualificationRefs);
            db.SaveChanges();

            foreach (EmployeeQualificationRef employeeQualificationRef in employeeQualificationRefs)
            {
                Console.WriteLine(employeeQualificationRef.EmployeeId);
                Console.WriteLine(employeeQualificationRef.QualificationId);
                Console.WriteLine("");
            }
        }
    }
}
