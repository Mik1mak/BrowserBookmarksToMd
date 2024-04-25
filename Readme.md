# Browser bookmarks converter
Simple tool for converting browser bookmarks from NETSCAPE-Bookmark-file-1 format to a folder and Markdown file structure.

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
  - --keep-empty       Keep empty files and folders
  - --record `<template>`  Template for bookmark record. {0}=name, {1}=href, {2}=add date. [default: `- [{0}]({1}) <sup><sub><sub>{2}</sub></sub></sup>`]
  - --output `<output dir>` 