using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EmployeManagement.Models;
using OfficeOpenXml; // For EPPlus Excel file parsing
using System.IO;
using System.Linq;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EmployeManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;

        public HomeController(ILogger<HomeController> logger, AppDbContext appDbContext,IMapper mapper)
        {
            _logger = logger;
            _appDbContext = appDbContext;
            _mapper = mapper;
        }

        public IActionResult Employee()
        {
            var list = _appDbContext.Employees.ToList();
            if (list.Count == 0)
            {
                list = new List<Employee>
                {
                    new Employee
                    {
                        Payroll_Number = "123456",
                        Forenames = "John",
                        Surname = "Doe",
                        Date_of_Birth = new DateTime(1980, 1, 1),
                        Telephone = "0123456789",
                        Mobile = "0123456789",
                        Address = "123 Main Street",
                        Address_2 = "Apt 1",
                        Postcode = "12345",
                        EMail_Home = ""
                    },
                    new Employee
                    {
                        Payroll_Number = "123457",
                        Forenames = "Jane",
                        Surname = "Doe",
                        Date_of_Birth = new DateTime(1980, 1, 1),
                        Telephone = "0123456789",
                        Mobile = "0123456789",
                        Address = "123 Main Street",
                        Address_2 = "Apt 1",
                        Postcode = "12345",
                        EMail_Home = ""
                    }
                };
            }
            return View(list);
        }

        [HttpPost]
        public IActionResult ImportEmployeeData(IFormFile fileUpload)
        {
            if (fileUpload != null && fileUpload.Length > 0)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileUpload.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    fileUpload.CopyTo(stream);
                }

                var employees = new List<Employee>();
                try
                {
                    if (fileUpload.FileName.EndsWith(".csv"))
                    {
                        using (var reader = new StreamReader(filePath))
                        using (var csv = new CsvHelper.CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)))
                        {
                            csv.Read();
                            csv.ReadHeader();
                    
                            while (csv.Read())
                            {
                                var employee = csv.GetRecord<EmployeeDto>();
                                    SaveToDatabase(employee); 
                            }
                        }
                    }
                    else if (fileUpload.FileName.EndsWith(".xlsx"))
                    {
                        using (var package = new ExcelPackage(new FileInfo(filePath)))
                        {
                            var worksheet = package.Workbook.Worksheets[0]; 
                            var rowCount = worksheet.Dimension.Rows;

                            for (int row = 2; row <= rowCount; row++) 
                            {
                                var employee = new Employee
                                {
                                    Payroll_Number = worksheet.Cells[row, 1].Text,
                                    Forenames = worksheet.Cells[row, 2].Text,
                                    Surname = worksheet.Cells[row, 3].Text,
                                    Date_of_Birth = DateTime.Parse(worksheet.Cells[row, 4].Text),
                                    Telephone = worksheet.Cells[row, 5].Text,
                                    Mobile = worksheet.Cells[row, 6].Text,
                                    Address = worksheet.Cells[row, 7].Text,
                                    Address_2 = worksheet.Cells[row, 8].Text,
                                    Postcode = worksheet.Cells[row, 9].Text,
                                    EMail_Home = worksheet.Cells[row, 10].Text,
                                    Start_Date = DateTime.Parse(worksheet.Cells[row, 11].Text)
                                };
                                employees.Add(employee);
                            }
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("fileUpload", "Invalid file format. Please upload a CSV or Excel file.");
                        return RedirectToAction("Employee");
                    }

                    if (employees.Any())
                    {
                        _appDbContext.Employees.AddRange(employees);
                        _appDbContext.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("fileUpload", "Error reading the file: " + ex.Message);
                    return RedirectToAction("Employee");
                }

                return RedirectToAction("Employee");
            }

            ModelState.AddModelError("fileUpload", "Please upload a file.");
            return RedirectToAction("Employee");
        }
        private void SaveToDatabase(EmployeeDto employeeDto)
        {
            
            Employee employee = _mapper.Map<Employee>(employeeDto);

            employee.Start_Date = employee.Start_Date.ToUniversalTime();
            employee.Date_of_Birth = employee.Date_of_Birth.ToUniversalTime();
            
            _appDbContext.Employees.Add(employee);
            _appDbContext.SaveChanges();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
