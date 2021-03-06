# 專案目標  
針對自己舊專案(網址如下) JobFilter 的爬蟲功能和操作介面進行改版。  
https://github.com/Jacky20200711/JobFilter  
&emsp;  
# 改版重點  
1.修改操作介面  
2.簡化操作流程(將部分的操作改用ajax來避免跳頁)  
3.去除排除關鍵字的功能(以封鎖工作來取代)  
4.改用 DB-First 規劃資料庫  
&emsp;  
# 使用技術  
1.使用 HttpClient 爬取網頁  
2.使用 AngleSharp 解析網頁  
3.使用 async / await 提升爬蟲的效率  
4.使用 EFCore 存取資料庫  
5.使用 Session 暫存過濾後的工作  
6.使用 CsvHelper 做資料的備份與還原  
7.使用 SweetAlert2 美化彈窗  
&emsp;  
# 開發環境  
Win10(家用版) + Visual Studio 2019 + .NET Core 3.1 MVC + SQL Server 2014 Express  
&emsp;  
# 安裝套件  
dotnet add package Microsoft.AspNetCore.Session --version 2.2.0  
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 5.0.7  
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 5.0.7  
dotnet add package AngleSharp --version 0.16.0  
dotnet add package CsvHelper --version 27.1.1  
dotnet add package NLog --version 4.7.10  
dotnet add package NLog.Web.AspNetCore --version 4.13.0-readme-preview  
&emsp;  
# DB Schema  
create table BlockJobItem  
(  
&nbsp;&nbsp;&nbsp;&nbsp;Id int primary key NOT NULL IDENTITY,  
&nbsp;&nbsp;&nbsp;&nbsp;JobCode nvarchar(20) NOT NULL,  
);  
&emsp;  
create table BlockCompany  
(  
&nbsp;&nbsp;&nbsp;&nbsp;Id int primary key NOT NULL IDENTITY,  
&nbsp;&nbsp;&nbsp;&nbsp;CompanyName nvarchar(100) NOT NULL,  
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
# 爬蟲列表  
![image](https://github.com/Jacky20200711/JobFilter2/blob/master/DEMO_02.PNG?raw=true)  
&emsp;  
# 修改爬蟲設定  
![image](https://github.com/Jacky20200711/JobFilter2/blob/master/DEMO_03.PNG?raw=true)  
&emsp;  
# 點選[爬取工作]，取得過濾後的工作列表  
![image](https://github.com/Jacky20200711/JobFilter2/blob/master/DEMO_04.PNG?raw=true)  
&emsp;  
# 點選職位名稱的超連結，開啟該職缺的104頁面  
![image](https://github.com/Jacky20200711/JobFilter2/blob/master/DEMO_05.PNG?raw=true)  
&emsp;  
# 點選封鎖公司之後，彈出選擇理由的視窗  
![image](https://github.com/Jacky20200711/JobFilter2/blob/master/DEMO_06.PNG?raw=true)  
&emsp;  
# 將DB的資料匯出成CSV檔案  
![image](https://github.com/Jacky20200711/JobFilter2/blob/master/DEMO_07.PNG?raw=true)  
&emsp;  
# 確認匯出結果  
![image](https://github.com/Jacky20200711/JobFilter2/blob/master/DEMO_08.PNG?raw=true)  
&emsp;  

