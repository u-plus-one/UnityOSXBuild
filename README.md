# UnityOSXBuild

Short description of the package.

## Usage

Install the package according to the installation instructions

...

## Installation

### Option 1: Package Manager

Open the Package Manager window, click on "Add Package from Git URL ...", then enter the following:
```
https://github.com/d3tonat0r/unityosxbuild.git
```

### Option 2: Manually Editing packages.json

Add the following line to your project's `Packages/manifest.json`:

```json
"com.github.d3tonat0r.unityosxbuild": "https://github.com/d3tonat0r/unityosxbuild.git"
```

### Option 3: Manual Installation (not recommended)

You can also download this repository and extract the `Editor` directory file anywhere inside your project's Assets folder. (The folder _must_ be named 'Editor' for it to work)

## The Problem

What the problem is

## The Solution

How this package solves the problem (zip)


### TODO

The following files will need to be edited:

- package.json
  - Package Name
  - Description
  - Package version
  - Minimum Unity version
  - Author information
  - Package samples
  - More information about package manifest files: https://docs.unity3d.com/Manual/upm-manifestPkg.html
- Runtime .asmdef
  - File name must be [company-name].[package-name].asmdef
  - Assembly name must be [company-name].[package-name]
- Editor .asmdef
  - File name must be "[company-name].[package-name].Editor.asmdef"
  - Assembly name must be "[company-name].[package-name].Editor"
  - Optional: Reference runtime assembly
- LICENSE.md
- This README.md

See https://docs.unity3d.com/Manual/cus-layout.html for more info.