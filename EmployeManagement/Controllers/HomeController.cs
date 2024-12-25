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

        public IActionResult Employee(string sortOrder, string searchString)
        {
            // Set up sorting order for the columns
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.SurnameSortParm = sortOrder == "Surname" ? "surname_desc" : "Surname";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

            var employees = from e in _appDbContext.Employees
                select e;

            // Search filter
            if (!String.IsNullOrEmpty(searchString))
            {
                employees = employees.Where(e => e.Surname.ToLower().Contains(searchString.ToLower())
                                                 || e.Forenames.ToLower().Contains(searchString.ToLower()));
            }

            // Sort based on selected order
            switch (sortOrder)
            {
                case "Surname":
                    employees = employees.OrderBy(e => e.Surname);  // Ascending order
                    break;
                case "surname_desc":
                    employees = employees.OrderByDescending(e => e.Surname);  // Descending order
                    break;
                case "Date":
                    employees = employees.OrderBy(e => e.Date_of_Birth);  // Ascending order by Date_of_Birth
                    break;
                case "date_desc":
                    employees = employees.OrderByDescending(e => e.Date_of_Birth);  // Descending order by Date_of_Birth
                    break;
                default:
                    employees = employees.OrderBy(e => e.Surname);  // Default sorting by Surname (ascending)
                    break;
            }

            return View(employees.ToList());
        }


        [HttpGet]
        public IActionResult EditEmployee(int id)
        {
            var employee = _appDbContext.Employees.Find(id);
            if (employee == null)
            {
                return NotFound();
            }
            return PartialView("_EditEmployeePartial", employee);
        }

        [HttpPost]
        public IActionResult EditEmployee(Employee employee)
        {
            if (ModelState.IsValid)
            {
                employee.Date_of_Birth = employee.Date_of_Birth.ToUniversalTime();
                employee.Start_Date = employee.Start_Date.ToUniversalTime();
                _appDbContext.Update(employee);
                _appDbContext.SaveChanges();
                return RedirectToAction("Employee");
            }
            return PartialView("_EditEmployeePartial", employee);
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
                int count = 0;
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
                                count++;
                            }
                            TempData["Message"] = $"{count} rows were successfully processed.";
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

                TempData["Message"] = $"{count} rows were successfully processed.";
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
