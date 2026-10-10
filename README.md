# MEP_Create_Duct_From_CAD_ver1

Independent Revit 2023/2024 (.NET Framework 4.8) command, moved from the Place Element From CAD project.
This project reads CAD imports/links to create Revit ducts.

## Build and load

Run in this folder:

```powershell
.\Build.ps1 -RevitVersion 2023
.\Build.ps1 -RevitVersion 2024
```

Output: `bin/Release/Revit<year>/net48/CreateDuctFromCad.dll`.
Add-In Manager entry: `CadLayerTools.CreateDuctFromCad`.
Choose the DLL matching your Revit version.

To register under Revit External Tools:

```powershell
.\Install.ps1 -RevitVersion 2023
```

Use 2024 instead for Revit 2024, then restart Revit. The manifest uses absolute DLL paths in this folder.
Keep this folder in place. Do not register the same command through multiple manifests.

## Workflow

Select the source model and CAD import/link, then choose a layer, analyze boundaries and configure round ducts.
The route table and overall bar show progress. Changes commit as one transaction; failed batches roll back.
After completion, choose whether to view/export the report. Return to the same configuration window and
continue with another layer. Cancel closes the session. Use Revit Undo to undo a committed batch.
Fittings are not connected automatically. UI messages use English; model-provided names keep their original text.

## Project boundaries

This project has its own source, artwork, build, install and manifest. It does not require the Place project.
Equipment placement, CAD block preview, family preview, equipment services and their command are excluded.
The source namespace stays `CadLayerTools` for compatibility; assembly name is `CreateDuctFromCad`.
Build output and local test/diagnostic files are ignored by Git.

## Git

Existing remote: `https://github.com/DinhHoaLe/MEP_Create_Duct_From_IFC_ver1.git`.
Existing IFC-file deletions were left as found during migration. No commit or push is performed by migration.