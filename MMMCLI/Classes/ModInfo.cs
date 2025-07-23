namespace MMMCLI;
public class ModInfo
{
    public string name { get; set; }
    public string author { get; set; }
    public string version { get; set; }
    public string install_location { get; set; }
    public string group { get; set; }
    public string[] dependencies { get; set; }
    public string git_path { get; set; }
    public string download_url { get; set; }
}