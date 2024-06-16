# 專案目標  
簡單的工作過濾器，用來進一步過濾104的工作列表。  
&emsp;  
# 使用技術  
1.使用 HttpClient 爬取網頁  
2.使用 AngleSharp 解析網頁  
3.使用 async / await 提升爬蟲的效率  
4.使用 EFCore 搭配 LINQ 存取資料庫  
5.使用 Session 暫存過濾後的工作  
6.使用 CsvHelper 做資料庫的備份與還原  
7.使用 SweetAlert2 美化彈窗  
&emsp;  
# 開發環境  
Win11 + Visual Studio 2022 + .NET Core MVC + SQL Server 2014 Express  
&emsp;  
# 安裝套件  
dotnet add package Microsoft.AspNetCore.Session --version 2.2.0  
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.2  
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.2  
dotnet add package AngleSharp --version 0.16.0  
dotnet add package CsvHelper --version 27.1.1  
dotnet add package NLog --version 4.7.10  
dotnet add package NLog.Web.AspNetCore --version 4.13.0-readme-preview  
&emsp;  
# 網站首頁(爬蟲設定列表)  
![image](https://github.com/Jacky20200711/JobFilter2/blob/master/DEMO_01.png?raw=true)  
&emsp;   
# 新增/修改爬蟲設定  
![image](https://github.com/Jacky20200711/JobFilter2/blob/master/DEMO_02.png?raw=true)  
&emsp;  
# 點選[爬取工作]，取得過濾後的工作列表  
![image](https://github.com/Jacky20200711/JobFilter2/blob/master/DEMO_03.png?raw=true)  
&emsp;  
# 點選[職位名稱的超連結]，開啟該職缺的104頁面  
![image](https://github.com/Jacky20200711/JobFilter2/blob/master/DEMO_04.png?raw=true)  
&emsp;  
# 點選[公司名稱的超連結]  
會自動以公司名稱到天眼通、goodjob、面試趣做搜尋，並開啟對應的分頁。  
&emsp;  
# 點選[公司地址的超連結]  
會開啟 Google 地圖的分頁，並定位到公司地址。  
&emsp;  

