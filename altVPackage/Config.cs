namespace altVPackage;

public class Config
{
    public string Branch { get; set; } = "release";
    public bool Windows { get; set; }
    public bool Server { get; set; }
    public bool Voice { get; set; }
    public bool CSharp { get; set; }
    public bool Js { get; set; }
    public bool JsByteCode { get; set; }
    public string OutputPath { get; set; } = "./";
}