# 專案目標  
針對自己舊專案 JobFilter 的爬蟲功能和操作介面進行改版  

# 改版重點
1.前端套版
2.使用比較簡潔、優雅的語法來解析工作內容
3.點選公司名稱時，可以自動到天眼通做搜尋
4.點選地址時，可以自動到 GoogleMap 做搜尋
5.新增封鎖工作的功能
6.改用 DB-First 來重新設計資料庫
7.爬取所有分頁

# 使用技術  
1.使用 HttpClient 爬取網頁  
2.使用 AngleSharp 解析網頁 
3.使用 EntityFrameworkCore 存取資料庫  
4.使用 Session 儲存過濾後的工作項目  

# 開發環境  
Win10(家用版) + Visual Studio 2019 + .NET Core 3.1 MVC + SQL Server 2014 Express  

# 安裝套件  
dotnet add package Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation --version 3.1.15  
dotnet add package Microsoft.AspNetCore.Session --version 2.2.0
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 5.0.7  
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 5.0.7  

# DB Schema  
CREATE DATABASE [JobFilter2];  
USE [JobFilter2];

create table BlockJobItem  
(  
    Id int primary key NOT NULL IDENTITY,  
    JobCode nvarchar(20) NOT NULL,  
    BlockReason varchar(20) NOT NULL,  
);  

create table BlockCompany  
(  
    Id int primary key NOT NULL IDENTITY,  
    CompanyName nvarchar(50) NOT NULL,  
    BlockReason varchar(20) NOT NULL,  
);

create table CrawlSetting  
(  
    Id int primary key NOT NULL IDENTITY,  
    TargetUrl nvarchar(400) NOT NULL,  
    MinSalary int NOT NULL,  
    Seniority nvarchar(10) NOT NULL,
);
 
