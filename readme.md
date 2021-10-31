# 專案目標  
針對自己舊專案 JobFilter 的爬蟲功能和操作介面進行改版。  
&emsp;  
# 改版重點  
1.前端套版 & 大幅修改操作介面  
2.使用比較簡潔、優雅的語法來解析工作內容  
3.點選公司名稱時，可以自動到天眼通做搜尋  
4.點選地址時，可以自動到 Google 地圖做搜尋  
5.新增封鎖工作的功能  
6.改用 DB-First 來重新設計資料庫  
&emsp;  
# 使用技術  
1.使用 HttpClient 爬取網頁  
2.使用 AngleSharp 解析網頁  
3.使用 async / await 提升爬取頁面的效率  
4.使用 EntityFrameworkCore 存取資料庫  
5.使用 Session 儲存過濾後的工作項目  
6.使用 CsvHelper 做資料的備份與還原  
&emsp;  
# 開發環境  
Win10(家用版) + Visual Studio 2019 + .NET Core 3.1 MVC + SQL Server 2014 Express  
&emsp;  
# 安裝套件  
dotnet add package Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation --version 3.1.15  
dotnet add package Microsoft.AspNetCore.Session --version 2.2.0  
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 5.0.7  
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 5.0.7  
dotnet add package AngleSharp --version 0.16.0  
dotnet add package CsvHelper --version 27.1.1  
&emsp;  
# DB Schema  
CREATE DATABASE [JobFilter2];  
USE [JobFilter2];  
&emsp;  
create table BlockJobItem  
(  
&nbsp;&nbsp;&nbsp;&nbsp;Id int primary key NOT NULL IDENTITY,  
&nbsp;&nbsp;&nbsp;&nbsp;JobCode nvarchar(20) NOT NULL,  
);  
&emsp;  
create table BlockCompany  
(  
&nbsp;&nbsp;&nbsp;&nbsp;Id int primary key NOT NULL IDENTITY,  
&nbsp;&nbsp;&nbsp;&nbsp;CompanyName nvarchar(50) NOT NULL,  
&nbsp;&nbsp;&nbsp;&nbsp;BlockReason nvarchar(20) NOT NULL,  
);  
&emsp;  
create table CrawlSetting  
(  
&nbsp;&nbsp;&nbsp;&nbsp;Id int primary key NOT NULL IDENTITY,  
&nbsp;&nbsp;&nbsp;&nbsp;TargetUrl nvarchar(400) NOT NULL,  
&nbsp;&nbsp;&nbsp;&nbsp;MinSalary int NOT NULL,  
&nbsp;&nbsp;&nbsp;&nbsp;Seniority nvarchar(10) NOT NULL,  
&nbsp;&nbsp;&nbsp;&nbsp;Remark nvarchar(20),  
);  
&emsp;  
# 網站首頁  
![image](https://github.com/Jacky20200711/JobFilter2/blob/master/DEMO_01.PNG?raw=true)  
&emsp;  
# 修改爬蟲設定  
![image](https://github.com/Jacky20200711/JobFilter2/blob/master/DEMO_02.PNG?raw=true)  
&emsp;  
# 執行爬蟲後，呈現的工作列表  
![image](https://github.com/Jacky20200711/JobFilter2/blob/master/DEMO_03.PNG?raw=true)  
&emsp;  
# 點選職位名稱，開啟新分頁如下  
![image](https://github.com/Jacky20200711/JobFilter2/blob/master/DEMO_04.PNG?raw=true)  
&emsp;  
# 點選公司地址，開啟新分頁如下  
![image](https://github.com/Jacky20200711/JobFilter2/blob/master/DEMO_05.PNG?raw=true)  
&emsp;  
