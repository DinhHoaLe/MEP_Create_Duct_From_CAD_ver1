# MEP_Create_Duct_From_IFC_ver1

Add-in C# cho Revit 2024, .NET Framework 4.8, x64: đọc IFC link, tạo/cập nhật Duct và tạo Air Terminal trong model chính.

## Gói gửi cho người dùng Revit 2024

Chạy `pwsh -NoProfile -File tools/Package-RevitAddin.ps1` để build và tạo `release/IFCInfo-Revit2024.zip`. Gửi file ZIP này; người nhận giải nén toàn bộ, đóng Revit, nhấn đúp `Install.cmd`, rồi mở Revit 2024 → Add-Ins → External Tools → IFC Info. Trình cài đặt chỉ cài cho tài khoản Windows hiện tại, không cần quyền Administrator hay pyRevit. Xem `HUONG_DAN.txt` trong ZIP.

## Cấu trúc

- `outputs/IFCInfo.csproj`: dự án add-in; mã nguồn trong `outputs/src/`.
- `outputs/IFCInfo.dll`: DLL phát hành, cập nhật sau khi build và kiểm tra thành công.
- `outputs/tests/`: kiểm tra reader, chống trùng, snapshot, connector và WPF.
- `tools/UiPreview/`: xem trước giao diện IFC/Duct và kiểm tra hồi quy WPF, không thao tác model.
- `tools/CadInspection/`: công cụ phụ đọc CAD/DXF, xem `tools/README.md`.

Dự án này không chứa CableTrayFromCad; công cụ IFC không phụ thuộc dự án Cable Tray bên cạnh.

## Hướng dẫn và kiểm tra

Xem [hướng dẫn sử dụng hiện tại](outputs/HUONG_DAN_CSHARP.txt), [phạm vi IFC](outputs/IFC_COMPATIBILITY.md), [đối chiếu Duct](outputs/DUCT_COMPARISON.md) và [ghi chú sửa lỗi](outputs/CHANGELOG.md).

Chạy từ thư mục gốc bằng PowerShell 7:

```powershell
dotnet build outputs/IFCInfo.csproj -c Release
pwsh -NoProfile -File outputs/tests/Test-IfcReader.ps1
pwsh -NoProfile -File outputs/tests/Test-DuctCoverage.ps1
pwsh -NoProfile -File outputs/tests/Test-DuctEnhancements.ps1
pwsh -NoProfile -File outputs/tests/Test-DuctUi.ps1
```

Cần cài Revit 2024 và .NET Framework 4.8 Developer Pack. Khi Revit nằm ở vị trí khác, truyền `-p:RevitApiDir="đường dẫn Revit 2024"` vào lệnh build. Các test logic cần PowerShell 7; kiểm tra WPF chạy bằng executable .NET Framework trên Windows.

Chức năng tạo/cập nhật và nối connector có sửa model. Test ngoài Revit không thay thế nghiệm thu trên RVT thử nghiệm; xem checklist trong `outputs/TIEN_DO_7_BUOC.md`.
