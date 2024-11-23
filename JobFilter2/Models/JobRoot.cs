using System.Collections.Generic;

public class JobData
{
    public string AppearDate { get; set; }
    public int ApplyCnt { get; set; }
    public long CoIndustry { get; set; }
    public string CoIndustryDesc { get; set; }
    public string CustName { get; set; }
    public string CustNo { get; set; }
    public string Description { get; set; }
    public string DescSnippet { get; set; }
    public double MrtDist { get; set; }
    public string JobAddress { get; set; }
    public long JobAddrNo { get; set; }
    public string JobAddrNoDesc { get; set; }
    public string JobName { get; set; }
    public string JobNameSnippet { get; set; }
    public string JobNo { get; set; }
    public int JobRo { get; set; }
    public int JobType { get; set; }
    public double Lat { get; set; }
    public double Lon { get; set; }
    public JobLink Link { get; set; }
    public List<string> Major { get; set; }
    public string Mrt { get; set; }
    public string MrtDesc { get; set; }
    public List<int> OptionEdu { get; set; }
    public int Period { get; set; }
    public int RemoteWorkType { get; set; }
    public int S10 { get; set; }
    public int SalaryHigh { get; set; }
    public int SalaryLow { get; set; }
    public JobTags Tags { get; set; }
    public List<int> S9 { get; set; }
    public int S5 { get; set; }
    public string D3 { get; set; }
    public object IsSave { get; set; }
    public object ApplyDate { get; set; }
    public object IsApply { get; set; }
}

public class JobLink
{
    public string Job { get; set; }
    public string Cust { get; set; }
    public string ApplyAnalyze { get; set; }
}

public class JobTags
{
    public ZoneForeign ZoneForeign { get; set; }
    public WfTags Wf10 { get; set; }
    public WfTags Wf13 { get; set; }
    public WfTags Wf34 { get; set; }
    public WfTags Wf2 { get; set; }
    public WfTags Wf8 { get; set; }
    public WfTags Wf11 { get; set; }
    public WfTags Wf28 { get; set; }
    public WfTags Wf33 { get; set; }
    public WfTags Wf29 { get; set; }
    public WfTags Wf1 { get; set; }
    public WfTags Wf12 { get; set; }
    public WfTags Wf26 { get; set; }
    public WfTags Wf31 { get; set; }
    public WfTags Wf7 { get; set; }
    public WfTags Wf30 { get; set; }
}

public class ZoneForeign
{
    public string Desc { get; set; }
    public int Param { get; set; }
}

public class WfTags
{
    public string Desc { get; set; }
    public string Param { get; set; }
}

public class Metadata
{
    public Pagination Pagination { get; set; }
    public FilterQuery FilterQuery { get; set; }
    public int PersonalBoost { get; set; }
}

public class Pagination
{
    public int Count { get; set; }
    public int CurrentPage { get; set; }
    public int LastPage { get; set; }
    public int Total { get; set; }
}

public class FilterQuery
{
    public int Order { get; set; }
    public int Asc { get; set; }
    public string Keyword { get; set; }
    public int Fz { get; set; }
    public int Kwop { get; set; }
    public int Isnew { get; set; }
    public int LangStatus { get; set; }
    public int SearchTempExclude { get; set; }
    public int RecommendJob { get; set; }
    public int HotJob { get; set; }
    public List<string> Area { get; set; }
    public List<string> Jobcat { get; set; }
    public List<string> ExcludeIndustryCat { get; set; }
    public List<string> ExpansionType { get; set; }
    public List<int> Ro { get; set; }
    public int Scstrict { get; set; }
    public int Scneg { get; set; }
    public int Page { get; set; }
    public int Pagesize { get; set; }
}

public class JobRoot
{
    public List<JobData> Data { get; set; }
    public Metadata Metadata { get; set; }
}
