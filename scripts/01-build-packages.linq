<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Xml.Linq</Namespace>
  <Namespace>LINQPad.Controls</Namespace>
  <IncludeUncapsulator>false</IncludeUncapsulator>
</Query>

#load ".\00-common"

DumpContainer dc = new DumpContainer().Dump();

async Task Main()
{
	await SetupAsync(QueryCancelToken);	
	Refresh();
}

void Refresh()
{
	dc.Content = LoadTable();
}

object LoadTable()
{
	return Projects
		.Select(x => new
		{
			Project = x.name, 
			Version = x.version, 
			Build = new Button("Build", o => Build(x))
		});
}

void Build(ProjectVersion p)
{
	string projPosition = Path.Combine(@"..", SearchProjectPathInSolutionFile(File.ReadAllText(@"..\Sdcb.WordCloud.slnx"), p.name));

	//DotNetRun($@"build ..\src\{project}\{project}.csproj -c Release");
	DotNetRun($@"pack {projPosition} -p:Version={p.version} -c Release -o .\nupkgs --include-symbols --include-source");
	Refresh();
}

public static string SearchProjectPathInSolutionFile(string solutionFileContent, string projectName)
{
	string projectSearchKey = projectName + ".csproj";

	try
	{
		XDocument solution = XDocument.Parse(solutionFileContent);
		string projectPath = solution.Root
			.Elements("Project")
			.Select(x => (string)x.Attribute("Path"))
			.FirstOrDefault(x => string.Equals(Path.GetFileName(x), projectSearchKey, StringComparison.OrdinalIgnoreCase));

		if (projectPath != null)
		{
			return projectPath;
		}
		else
		{
			throw new Exception("Project path could not be found in the solution file.");
		}
	}
	catch (Exception ex)
	{
		throw new Exception("An error occurred while searching the project path in the solution file.", ex);
	}
}
