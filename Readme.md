# Browser bookmarks converter
Simple tool for converting browser bookmarks from NETSCAPE-Bookmark-file-1 format to a folder and Markdown file structure.

![example](https://raw.githubusercontent.com/Mik1mak/BrowserBookmarksToMd/3f60a4c1e815667ec8b626c9a51c55b09d95707a/Example.png)
*In order: browser bookmarks view, part of folder structure as a result of tool operation, readme file from one of the directories*

## Installation
```
dotnet tool install --global BrowserBookmarksToMd
```
## Usage
  ```
  convert-bookmarks <Bookmarks file> [options]
  ```
### Arguments
  - `<Bookmarks file>`  File with browser bookmarks
### Options
  - `--keep-empty` Keep empty files and folders
  - `--record <template>`  Template for bookmark record. {0}=name, {1}=href, {2}=add date. (default: `- [{0}]({1}) <sup><sub><sub>{2}</sub></sub></sup>`)
  - `--output <output dir>` 