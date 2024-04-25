using System.CommandLine;

var filePathArg = new Argument<FileInfo>("Bookmarks", "File with browser bookmarks");

var rootCommand = new RootCommand("Converts HTML file with browser bookmarks to folders and Markdown files.")
{
    filePathArg
};

rootCommand.SetHandler((FileInfo bookmarks) => { 
    List<DirectoryInfo> createdDictionaries = new();
    List<FileInfo> createdFiles = new();

    // TODO: handler;
    
}, filePathArg);

await rootCommand.InvokeAsync(args);