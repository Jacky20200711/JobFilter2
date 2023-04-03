# 專案目標  
針對自己舊專案(網址如下) JobFilter 的爬蟲功能和操作介面進行改版。  
https://github.com/Jacky20200711/JobFilter  
&emsp;  
# 改版重點  
1.修改操作介面(練習套版)  
2.優化操作流程(將多數的操作改用 AJAX 來避免跳頁)  
3.新增封鎖工作的功能  
&emsp;  
# 使用技術  
1.使用 HttpClient 爬取網頁  
2.使用 AngleSharp 解析網頁  
3.使用 async / await 提升爬蟲的效率  
4.使用 EFCore + LINQ 存取資料庫  
5.使用 Session 暫存過濾後的工作  
6.使用 CsvHelper 做資料庫的備份與還原  
7.使用 SweetAlert2 美化彈窗  
&emsp;  
# 開發環境  
Win10(家用版) + Visual Studio 2019/2022 + .NET Core 3.1 MVC + SQL Server 2014 Express  
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
# 網站首頁(爬蟲列表)  
![image](https://github.com/Jacky20200711/JobFilter2/blob/master/DEMO_01.PNG?raw=true)  
&emsp;   
# 新增/修改爬蟲設定  
![image](https://github.com/Jacky20200711/JobFilter2/blob/master/DEMO_02.PNG?raw=true)  
&emsp;  
# 點選[爬取工作]，取得過濾後的工作列表  
![image](https://github.com/Jacky20200711/JobFilter2/blob/master/DEMO_03.PNG?raw=true)  
&emsp;  
# 點選[職位名稱的超連結]，開啟該職缺的104頁面  
![image](https://github.com/Jacky20200711/JobFilter2/blob/master/DEMO_04.PNG?raw=true)  
&emsp;  
# 點選[封鎖公司]，彈出選擇理由的視窗  
![image](https://github.com/Jacky20200711/JobFilter2/blob/master/DEMO_05.PNG?raw=true)  
&emsp;  

