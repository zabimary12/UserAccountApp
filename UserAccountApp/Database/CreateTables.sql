CREATE TABLE Companies (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Address NVARCHAR(200),
    PhoneNumber NVARCHAR(20)
);

CREATE TABLE Departments (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL
);

CREATE TABLE Positions (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL
);

CREATE TABLE Employees (
    Id INT PRIMARY KEY IDENTITY(1,1),
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    MiddleName NVARCHAR(50),
    Address NVARCHAR(200),
    PhoneNumber NVARCHAR(20),
    BirthDate DATE NOT NULL,
    HireDate DATE NOT NULL,
    Salary DECIMAL(18,2) NOT NULL,
    DepartmentId INT NOT NULL,
    PositionId INT NOT NULL,
    CompanyId INT NOT NULL,
    FOREIGN KEY (DepartmentId) REFERENCES Departments(Id),
    FOREIGN KEY (PositionId) REFERENCES Positions(Id),
    FOREIGN KEY (CompanyId) REFERENCES Companies(Id)
); 