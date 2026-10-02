using System.Collections.Generic;

namespace IFCInfo
{
    /// <summary>Yêu cầu tạo Duct sau khi người dùng chọn type và ánh xạ hệ thống.</summary>
    public sealed class DuctRequest
    {
        // Ánh xạ riêng theo tiết diện để hai nhóm có thể chọn hệ thống độc lập.
        public static string SystemKey(bool round, string sourceType) => (round ? "Round|" : "Rectangular|") + (sourceType ?? "");
        public static string SystemKey(string shape, string sourceType) => shape + "|" + (sourceType ?? "");
        public static string ShapeLabel(string shape) => shape == "Round" ? "Tròn" : shape == "Square" ? "Vuông" : shape == "Oval" ? "Oval" : shape == "Rectangular" ? "Chữ nhật" : "Chưa xác định";
        public List<DuctPlanItem> Items;
        public long RoundTypeId, RectangularTypeId;
        public Dictionary<string, long> SystemTypes = new Dictionary<string, long>();
        public Dictionary<string, long> DuctTypes = new Dictionary<string, long>();
        public Dictionary<string, NewDuctSystem> NewSystems = new Dictionary<string, NewDuctSystem>();
        public Dictionary<string, long> Levels = new Dictionary<string, long>();
        public Dictionary<string, long> Worksets = new Dictionary<string, long>();
        public bool KeepSuccessful, CreateFittings, ConnectTerminals, SaveSettings = true;
        public double FittingGapMm = 1;
        public List<DuctRunRow> Skipped = new List<DuctRunRow>();
    }
    public sealed class NewDuctSystem
    {
        public string Name;
        public long TemplateId;
        public string Classification;
    }
}

