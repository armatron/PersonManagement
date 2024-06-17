# PersonManagement

Supports basic functionality for magage persons. Each person have common attributes such as: gender, first name, last name,
number, identity number, email, phone, dewscription... Each person can also have multiple address and tax number based on different countries. 

# Technology
Project is built in .NET 8, ASP.NET Core Web App (Model-View-Controller) with CRUD operations. It uses Entity Framework for database queries.
All required data are sored in MySQL database.

# Implementation
Projec is devided into four parts:
1. Module for nanaging basic persons data, tax numbers and addresses
2. Module for nanaging countries
3. Module for mamaging tax numbers
4. Module for managing addresses

Each module contains filter to search data id sorting data.


# Rerqirements
1. Visual Studio 2022
2. .Net 8
3. Entity Framework
4. MySQL Workbench


# Run from Visual Studio
1. Install required programs and tools
2. Fetch files to local path
3. Open MySQL Workbench and create database with name pm.
4. In MySQL Workbench execute script Install/db_create_tables.sql to create db tables. To insert test data into each table, execute scripts Install/pm_*.sql
5. In Visual Studio open appsettings.json and modify connectionstring.
6. In Visual Studio hit F5.


# TODO:
1. Prevent constrain delete
2. Simultaneous filtering and sorting
3. Pagination
4. Validations for foreign entities
5. Localization
6. Auditing
7. Error handling (Log file) and alerting (SMTP)