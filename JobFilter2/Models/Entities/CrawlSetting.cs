﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace JobFilter2.Models.Entities;

public partial class CrawlSetting
{
    /// <summary>
    /// 資料編號
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 目標網址
    /// </summary>
    public string TargetUrl { get; set; }

    /// <summary>
    /// 最低月薪(不得低於)
    /// </summary>
    public int MinSalary { get; set; }

    /// <summary>
    /// 最高月薪(不得低於)
    /// </summary>
    public int MaxSalary { get; set; }

    /// <summary>
    /// 年資
    /// </summary>
    public string Seniority { get; set; }

    /// <summary>
    /// 設定說明
    /// </summary>
    public string Remark { get; set; }

    /// <summary>
    /// 排除關鍵字(以逗號區隔)
    /// </summary>
    public string ExcludeWords { get; set; }

    /// <summary>
    /// 包含關鍵字(以逗號區隔)
    /// </summary>
    public string IncludeWords { get; set; }

    /// <summary>
    /// 是否有寫薪水(意即是否排除面議)
    /// </summary>
    public string HasSalary { get; set; }
}