using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace IFCInfo
{
    internal sealed class DuctGroupEditor : Expander
    {
        internal ComboBox TypeBox, SystemBox;
        private readonly CheckBox createNew;
        private readonly TextBox name;
        private readonly ComboBox template, classification;
        internal string Key { get; }
        internal string Label { get; }

        internal DuctGroupEditor(string key, string shape, string sourceSystem, int count,
            List<DuctChoice> types, List<DuctChoice> systems, DuctSettingsData saved, bool expanded)
        {
            Key = key;
            Label = DuctRequest.ShapeLabel(shape) + " · " + (string.IsNullOrWhiteSpace(sourceSystem) ? "Không có System Type" : sourceSystem);
            Header = Label + " · " + count + " đoạn";
            IsExpanded = expanded;
            Margin = new Thickness(0, 0, 0, 12);
            Padding = new Thickness(12);
            Background = System.Windows.Media.Brushes.White;
            Foreground = IFCInfoWindow.Brush("#37322B");
            BorderBrush = IFCInfoWindow.Brush("#DDD6CE");
            BorderThickness = new Thickness(1);
            var body = new StackPanel();
            Content = body;
            TypeBox = UiDesign.Field(body, "Duct Type", "Chỉ hiển thị type đúng tiết diện · " + count + " đoạn", "Chọn Duct Type…", false);
            TypeBox.ItemsSource = types;
            TypeBox.Name = "GroupDuctType";
            long savedType = saved.DuctTypes.FirstOrDefault(x => x.Key == key)?.Id ??
                (shape == "Round" ? saved.RoundTypeId : shape == "Oval" ? 0 : saved.RectangularTypeId);
            TypeBox.SelectedItem = types.FirstOrDefault(x => x.Id == savedType) ?? (types.Count == 1 ? types[0] : null);
            if (types.Count == 0) body.Children.Add(IFCInfoWindow.Text("Model chưa có Duct Type cho tiết diện này. Nạp type phù hợp rồi mở lại công cụ.", 12, "#9A5B12"));
            SystemBox = UiDesign.Field(body, "System Type đích", "Gán hệ thống Revit riêng cho nhóm này", "Chọn System Type…", true);
            SystemBox.ItemsSource = systems;
            SystemBox.Name = "GroupSystemType";
            long savedSystem = saved.Systems.FirstOrDefault(x => x.Key == key)?.Id ?? 0;
            SystemBox.SelectedItem = systems.FirstOrDefault(x => x.Id == savedSystem) ??
                systems.FirstOrDefault(x => string.Equals(x.Name, sourceSystem, StringComparison.OrdinalIgnoreCase));
            createNew = new CheckBox { Name = "CreateNewSystem", Content = "Tạo System Type mới", Margin = new Thickness(0, 14, 0, 8) };
            body.Children.Add(createNew);
            var fields = new StackPanel { Visibility = Visibility.Collapsed };
            body.Children.Add(fields);
            fields.Children.Add(IFCInfoWindow.Text("Tên System Type mới", 13, "#37322B"));
            name = new TextBox { Text = sourceSystem == "Không có thông tin" ? "" : sourceSystem,
                MinHeight = 34, Padding = new Thickness(8), Margin = new Thickness(0, 6, 0, 8) };
            fields.Children.Add(name);
            name.Name = "NewSystemName";
            template = UiDesign.Field(fields, "Hệ thống mẫu", "Kế thừa phân loại và thiết lập từ hệ thống mẫu", "Chọn mẫu…", true);
            var templates = new List<DuctChoice> { new DuctChoice { Id = 0, Name = "Tạo từ phân loại hệ thống" } };
            templates.AddRange(systems);
            template.ItemsSource = templates; template.SelectedIndex = 0;
            template.Name = "SystemTemplate";
            classification = UiDesign.Field(fields, "System Classification", "Chọn rõ chức năng của hệ thống mới", "Chọn phân loại…", true);
            classification.ItemsSource = new[] { "SupplyAir", "ReturnAir", "ExhaustAir", "OtherAir" };
            classification.Name = "SystemClassification";
            template.SelectionChanged += (s, e) => classification.IsEnabled = ((DuctChoice)template.SelectedItem).Id == 0;
            createNew.Checked += (s, e) => { fields.Visibility = Visibility.Visible; SystemBox.IsEnabled = false; };
            createNew.Unchecked += (s, e) => { fields.Visibility = Visibility.Collapsed; SystemBox.IsEnabled = true; };
            fields.Children.Add(IFCInfoWindow.Text("Chỉ tạo hệ thống khi bấm Tạo Duct. Các nhóm dùng cùng tên và phân loại sẽ dùng chung hệ thống mới.", 12, "#716B63"));
        }

        internal NewDuctSystem NewSystem => createNew.IsChecked == true ? new NewDuctSystem
        {
            Name = name.Text.Trim(), TemplateId = ((DuctChoice)template.SelectedItem).Id,
            Classification = classification.SelectedItem as string
        } : null;

        internal string Validate()
        {
            if (TypeBox.SelectedItem == null) return Label + ": chọn Duct Type đúng tiết diện.";
            var proposed = NewSystem;
            if (proposed == null) return SystemBox.SelectedItem == null ? Label + ": chọn System Type hoặc tạo mới." : null;
            if (string.IsNullOrWhiteSpace(proposed.Name)) return Label + ": nhập tên System Type mới.";
            if (proposed.Name.IndexOfAny(new[] { '\\', '/', ':', '{', '}', '[', ']', '|', ';', '<', '>', '?', '`', '~' }) >= 0)
                return Label + ": tên System Type chứa ký tự không hợp lệ.";
            if (SystemBox.Items.Cast<DuctChoice>().Any(x => string.Equals(x.Name, proposed.Name, StringComparison.OrdinalIgnoreCase)))
                return Label + ": tên đã tồn tại. Bỏ chọn Tạo mới và chọn hệ thống có sẵn.";
            if (proposed.TemplateId == 0 && proposed.Classification == null) return Label + ": chọn phân loại hệ thống mới.";
            return null;
        }
    }
}
