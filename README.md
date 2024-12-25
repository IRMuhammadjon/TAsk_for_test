You are given a CSV file – this is the input from an external system
we have 2 extarnal files for testing .
Create a SQL Server database with only one table – Employees. That table should have as many columns as needed to accommodate data from the file.
CREATE TABLE Employees (
    Id INT IDENTITY(1,1) PRIMARY KEY,               -- Auto-incrementing primary key
    Payroll_Number NVARCHAR(50) NOT NULL,           -- Payroll number
    Forenames NVARCHAR(100) NOT NULL,               -- Forenames
    Surname NVARCHAR(100) NOT NULL,                 -- Surname
    Date_of_Birth DATE NOT NULL,                    -- Date of birth
    Telephone NVARCHAR(20) NULL,                   -- Telephone (optional)
    Mobile NVARCHAR(20) NULL,                      -- Mobile (optional)
    Address NVARCHAR(255) NOT NULL,                -- Address
    Address_2 NVARCHAR(255) NULL,                  -- Address line 2 (optional)
    Postcode NVARCHAR(20) NOT NULL,                -- Postcode
    EMail_Home NVARCHAR(255) NULL,                 -- Email (optional)
    Start_Date DATE NOT NULL                       -- Start date
);
Create a simple ASP.Net MVC website with one page. That page should contain 
Browse File control
a button to execute the import 
a grid/table (described below).
![working fueters](https://github.com/user-attachments/assets/ab69cbf7-6509-4cb7-87d0-4bd5356ef791)
When the user selects the file and clicks on the Import button, the program should parse it, get data and insert it into the database. The page should report on how many rows were successfully processed.
The added rows should be shown in the grid on the same page. Data should be sorted by surname ascending. The grid should support sorting, searching, and editing functionality.
You are advised to use a third-party library for the grid.
