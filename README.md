# sync-files

sync-files is a .NET console application designed to synchronize files between a source and destination directory. It's particularly useful for working on CircuitPython code without directly handling files on the board.

## Features

- Watches a specified source directory and synchronizes files to a destination directory.
- Supports a variety of file types with customizable filters.
- Offers both a config file and command-line argument options for flexibility.
- Initialization option to clear the destination directory and recreate structure based on source.

## Requirements

- .NET 8

## Usage

### Using Command-Line Arguments

Use command-line arguments as follows:

	--source "C:\source" 
	--destination "C:\destination" 
	--filters "*.py" "*.mpy" "*.toml" "*.html"
	--initialise
	--keepFiles "boot_out.txt" "settings.toml"
	--config "C:\config"

### Using a Config File

It looks for a `config.json` file in the same directory as the executable or in the `--config` directory with the following structure:

```json
{
  "source": "D:\\Source",
  "destination": "D:\\Destination",
  "initialise": true,
  "filters": [ "*.py", "*.mpy", "*.toml", "*.html" ],
  "filesToKeep": [ "boot_out.txt", "settings.toml" ]
}
```

Note: If a config.json file is present, command-line arguments other than `--config` will be ignored.

### Installation

This is a self-contained .NET single executable application. No installation is required. Simply download and run the .exe file on a compatible system.

### Contributing

Contributions are welcome. Please feel free to fork, modify, and send pull requests.

### License
MIT License

Copyright (c) 2024 Ben Robinson

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

