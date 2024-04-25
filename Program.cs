using System.Text;
using System.CommandLine;
using HtmlAgilityPack;

var filePathArg = new Argument<FileInfo>(
    name: "Bookmarks file", 
    description: "File with browser bookmarks.",
    isDefault: false,
    parse: result => {
        return new FileInfo(result.Tokens.Single().Value);
    });

var keepEmptyOption = new Option<bool>("--keep-empty", "Keep empty files and folders.")
{
    IsRequired = false,
};

var recordOption = new Option<string>("--record",
    getDefaultValue: () => "- [{0}]({1}) <sup><sub><sub>{2}</sub></sub></sup>",
    description: "Template for bookmark record. {0}=name, {1}=href, {2}=add date.")
{
    IsRequired = false,
};

var outputOption = new Option<DirectoryInfo>("--output", 
    isDefault: true,
    parseArgument: result => {
        return result.Tokens.Any() ? new DirectoryInfo(result.Tokens.Single().Value) : new DirectoryInfo(Environment.CurrentDirectory);
    })
{
    IsRequired = false,
};

var rootCommand = new RootCommand("Simple tool for converting browser bookmarks from NETSCAPE-Bookmark-file-1 format to a folder and Markdown file structure.")
{
    filePathArg,
    keepEmptyOption,
    recordOption,
    outputOption,
};

rootCommand.SetHandler(async (FileInfo bookmarksSource, bool keepEmpty, string template, DirectoryInfo output) => { 
    HashSet<DirectoryInfo> createdDictionaries = new();
    HashSet<FileInfo> createdFiles = new();
    Dictionary<DirectoryInfo, StreamWriter> fileWriters = new();

    bool rollback = false;

    async Task ProcessNode(HtmlNode currentNode, DirectoryInfo currentDir) 
    {
        DirectoryInfo thisLevelDir = currentDir;

        if(currentNode.Name == "dl")
            foreach(HtmlNode node in currentNode.ChildNodes)
            {
                if(node.Name == "dt")
                {
                    if(node.FirstChild.Name == "h3")
                    {
                        HtmlNode h3 = node.FirstChild;

                        DirectoryInfo newDir = Directory.CreateDirectory(Path.Combine(thisLevelDir.FullName, h3.InnerText));
                        createdDictionaries.Add(newDir);

                        newDir.CreationTimeUtc = DateTimeOffset.FromUnixTimeSeconds(int.Parse(h3.Attributes["ADD_DATE"].Value)).UtcDateTime;
                        int unixTimeSecondsLastWrite = int.Parse(h3.Attributes["LAST_MODIFIED"].Value);
                        newDir.LastWriteTimeUtc = unixTimeSecondsLastWrite == 0 
                            ? newDir.CreationTimeUtc 
                            : DateTimeOffset.FromUnixTimeSeconds(unixTimeSecondsLastWrite).UtcDateTime;

                        FileInfo newMdFile = new(Path.Combine(newDir.FullName, $"Readme.md"));
                        createdFiles.Add(newMdFile);
                        fileWriters.Add(newDir, newMdFile.CreateText());

                        currentDir = newDir;
                    }
                    if(node.FirstChild.Name == "a")
                    {
                        HtmlNode a = node.FirstChild;
                        string href = a.Attributes["HREF"].Value;
                        string addDate = DateTimeOffset.FromUnixTimeSeconds(int.Parse(a.Attributes["ADD_DATE"].Value))
                            .LocalDateTime.ToString("yyyy-MM-dd HH:mm");

                        await fileWriters[currentDir].WriteLineAsync(string.Format(template, a.InnerText, href, addDate));
                    }
                }
                else if(node.Name == "dl")
                {
                    await ProcessNode(node, currentDir);
                }

            }
    }

    try
    {
        DirectoryInfo workingDir = Directory.CreateDirectory(Path.Combine(output.FullName,  bookmarksSource.Name.Replace(".html", "")));
        createdDictionaries.Add(workingDir);

        StringBuilder validHtmlDoc = new();
        await foreach (string line in File.ReadLinesAsync(bookmarksSource.FullName))
        {
            validHtmlDoc.Append(line.Replace("<p>", ""));
            if(line.Contains("<DT>"))
                validHtmlDoc.Append("</DT>");
            validHtmlDoc.AppendLine();
        }

        HtmlDocument bookmarksDoc = new();
        bookmarksDoc.LoadHtml(validHtmlDoc.ToString());
        HtmlNode mainDl = bookmarksDoc.DocumentNode.SelectSingleNode("/dl");

        await ProcessNode(mainDl, workingDir);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Bookmarks has beed successfully converted to Markdown files: {workingDir.FullName}");
    }
    catch(Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(ex);

        rollback = true;
    }
    finally
    {
        Console.ResetColor();

        foreach(var (_, stream) in fileWriters)
            stream.Close();
    }

    foreach(FileInfo file in createdFiles)
        if(rollback || (!keepEmpty && file.Length == 0))
            file.Delete();

    foreach(DirectoryInfo dir in createdDictionaries.Reverse())
        if(rollback || (!keepEmpty && !dir.EnumerateFiles().Any() && !dir.EnumerateDirectories().Any()))
            dir.Delete();
    
}, filePathArg, keepEmptyOption, recordOption, outputOption);

await rootCommand.InvokeAsync(args);