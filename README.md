# SPT Killfeed

Recompile of Drexira's DragonDen Killfeed reference mod for spt 4.1

## Building

The project targets `netstandard2.1` and references the required EFT, SPT, Unity, Harmony, and BepInEx assemblies in place from a read-only SPT installation through the `SptPath` MSBuild property. Proprietary game assemblies are not copied to the build output or release archive.

Create an ignored `Directory.Build.props.user` in the repository root:

```xml
<Project>
  <PropertyGroup>
    <SptPath>X:\Path\To\SPT</SptPath>
  </PropertyGroup>
</Project>
```

Build and package the plugin with:

```powershell
pwsh -File .\scripts\build-release.ps1
```

The SPT path may also be supplied explicitly:

```powershell
pwsh -File .\scripts\build-release.ps1 -SptPath 'X:\Path\To\SPT'
```

After dependencies have been restored once, pass `-NoRestore` for an offline build. The archive is written to `dist/SPT-Killfeed-4.1.0.zip`.

## Changelog

4.1.0 - rekease for SPT 4.1.5

## Licence

MIT