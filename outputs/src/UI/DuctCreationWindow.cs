using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace IFCInfo
{
    public sealed class DuctCreationWindow : Window
    {
        public DuctRequest Request
        {
            get; private set;
        }
        public DuctCreationWindow(List<DuctPlanItem> items, List<string> issues,
            List<DuctChoice> roundTypes, List<DuctChoice> rectangularTypes, List<DuctChoice> systems,
            List<DuctChoice> levels, List<DuctChoice> worksets, DuctSettingsData saved, bool updating = false, List<DuctChoice> ovalTypes = null)
        {
            Title = updating ? "Xem trước cập nhật Duct từ IFC" : "Bước 3 · Tạo Duct từ IFC";
            Width = 1200;
            Height = 690;
            MinWidth = 900;
            MinHeight = 440;
            MaxHeight = SystemParameters.WorkArea.Height;
            MaxWidth = SystemParameters.WorkArea.Width;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = UiDesign.Background;
            UseLayoutRounding = true;
            FontFamily = new System.Windows.Media.FontFamily("Segoe UI");
            FontSize = 14;
            var root = new DockPanel { Margin = new Thickness(28), Background = Background };
            UiDesign.SetContent(this, root);
            var footer = new StackPanel { Margin = new Thickness(0, 14, 0, 0) };
            DockPanel.SetDock(footer, Dock.Bottom);
            root.Children.Add(footer);
            var feedback = IFCInfoWindow.Text("", 13, "#9A5B12");
            footer.Children.Add(feedback);
            var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var back = IFCInfoWindow.Button("Quay lại", false);
            back.IsCancel = true;
            back.Click += (s, e) => Close();
            buttons.Children.Add(back);
            var create = IFCInfoWindow.Button((updating ? "Cập nhật " : "Tạo ") + items.Count + " Duct", true);
            create.IsEnabled = items.Count > 0;
            buttons.Children.Add(create);
            footer.Children.Add(buttons);
            var header = new Grid { Margin = new Thickness(0,0,0,22) };
            header.ColumnDefinitions.Add(new ColumnDefinition());
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var titlePanel = new StackPanel(); header.Children.Add(titlePanel);
            var title = IFCInfoWindow.Text(updating ? "Cập nhật Duct từ IFC" : "Thiết lập tạo Duct", 27, "#37322B");
            title.FontWeight = FontWeights.Bold; titlePanel.Children.Add(title);
            titlePanel.Children.Add(IFCInfoWindow.Text("Bước 3 · Kiểm tra và tạo ống", 14, "#716B63"));
            var steps = new ContentControl { Content = UiDesign.Steps(3), Margin = new Thickness(12,0,0,0) };
            Grid.SetColumn(steps,1); header.Children.Add(steps);
            header.SizeChanged += (s,e) => steps.Visibility = header.ActualWidth < 780 ? Visibility.Collapsed : Visibility.Visible;
            DockPanel.SetDock(header,Dock.Top); root.Children.Add(header);
            var body = new StackPanel();
            var columns = new Grid();
            columns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(340), MinWidth = 280 });
            columns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(25) });
            columns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 420 });
            root.Children.Add(columns);
            var leftScroll = new ScrollViewer { Content = body, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, Padding = new Thickness(0,0,10,0) };
            columns.Children.Add(leftScroll);
            var divider = new Border { Width = 1, Background = IFCInfoWindow.Brush("#DDD6CE"), Margin = new Thickness(12,0,12,0) };
            Grid.SetColumn(divider,1); columns.Children.Add(divider);
            var groupsBody = new StackPanel();
            var rightScroll = new ScrollViewer { Content = groupsBody, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, Padding = new Thickness(0,0,8,0) };
            Grid.SetColumn(rightScroll,2); columns.Children.Add(rightScroll);
            body.Children.Add(IFCInfoWindow.Text("LEVEL & TÙY CHỌN CHUNG", 16, "#37322B"));
            groupsBody.Children.Add(IFCInfoWindow.Text("ÁNH XẠ THEO SHAPE × SYSTEM TYPE", 18, "#37322B"));
            groupsBody.Children.Add(IFCInfoWindow.Text("Mỗi nhóm dùng Duct Type và System Type riêng. Mở nhóm để thiết lập; bỏ chọn các nguồn chưa muốn tạo ở bảng phía dưới.",13,"#716B63"));
            var summary = new Border { Background = IFCInfoWindow.Brush("#E8E2DA"), CornerRadius = new CornerRadius(10), Padding = new Thickness(18,12,18,12), Margin = new Thickness(0,0,0,16) };
            summary.Child = IFCInfoWindow.Text((updating ? "Có thay đổi: " : "Sẵn sàng tạo: ") + items.Count + " đoạn     ·     Bỏ qua  " + issues.Count + " đoạn", 16, "#6F6044");
            groupsBody.Children.Add(summary);
            var editors = new Dictionary<string, DuctGroupEditor>();
            var groupSearch = new TextBox { MinHeight=34, Padding=new Thickness(8), Margin=new Thickness(0,12,0,12), ToolTip="Tìm nhóm theo Shape hoặc System Type" };
            groupsBody.Children.Add(IFCInfoWindow.Text("Tìm nhóm Shape / System Type",12,"#716B63"));
            groupsBody.Children.Add(groupSearch);
            groupSearch.TextChanged += (s,e) =>
            {
                foreach(var editor in editors.Values)
                    editor.Visibility=editor.Label.IndexOf(groupSearch.Text.Trim(),StringComparison.OrdinalIgnoreCase)>=0 ? Visibility.Visible : Visibility.Collapsed;
            };
            var groups = items.GroupBy(i => i.GroupKey).OrderBy(g => g.Key).ToList();
            foreach (var group in groups)
            {
                var first = group.First();
                var available = first.Round ? roundTypes : first.Oval ? (ovalTypes ?? new List<DuctChoice>()) : rectangularTypes;
                var editor = new DuctGroupEditor(group.Key, first.ShapeKey, first.Source.SystemType, group.Count(), available, systems, saved, groups.Count == 1);
                editors.Add(group.Key, editor);
                groupsBody.Children.Add(editor);
            }
            var levelMapping = new Dictionary<string, ComboBox>();
            var manualLevel = new CheckBox { Content="Manual · Ánh xạ Level nguồn sang Level đích", IsChecked=saved.Levels.Any(m=>m.Id>0), Margin=new Thickness(0,16,0,8) };
            body.Children.Add(manualLevel);
            body.Children.Add(IFCInfoWindow.Text("Tắt Manual: tự chọn Level dưới cao độ ống. Đường tim IFC được giữ nguyên.",12,"#716B63"));
            manualLevel.Checked += (s,e) => { foreach(var box in levelMapping.Values) box.IsEnabled=true; };
            manualLevel.Unchecked += (s,e) => { foreach(var box in levelMapping.Values) box.IsEnabled=false; };
            foreach (var key in items.Select(i=>i.LevelKey ?? "Không có Level nguồn").Distinct())
            {
                var box = UiDesign.Field(body, "Level nguồn: " + key, "Tự động chọn Level dưới cao độ ống hoặc ánh xạ Level đích", "Level", false);
                var choices = new List<DuctChoice> { new DuctChoice { Id = 0, Name = "Tự động theo cao độ" } }; choices.AddRange(levels);
                box.ItemsSource = choices;
                box.SelectedItem = choices.FirstOrDefault(c=>c.Id == saved.Levels.FirstOrDefault(m=>m.Key == key)?.Id) ?? choices[0];
                levelMapping[key] = box;
                box.IsEnabled=manualLevel.IsChecked==true;
            }
            var worksetMapping = new Dictionary<string, ComboBox>();
            if (worksets.Count > 0)
            foreach (var key in editors.Keys)
            {
                var box = UiDesign.Field(body, "Workset: " + editors[key].Label, "Ánh xạ theo nhóm tiết diện và hệ thống", "Workset", false);
                var choices = new List<DuctChoice> { new DuctChoice { Id = 0, Name = "Workset hiện hành / giữ Workset khi cập nhật" } }; choices.AddRange(worksets);
                box.ItemsSource = choices;
                box.SelectedItem = choices.FirstOrDefault(c=>c.Id == saved.Worksets.FirstOrDefault(m=>m.Key == key)?.Id) ?? choices[0];
                worksetMapping[key] = box;
            }
            var policy = UiDesign.Field(body, "Khi có lỗi", "Áp dụng cho cả tạo duct và nối fitting", "Chọn cách xử lý", false);
            policy.ItemsSource = new[] { "Hoàn tác toàn bộ", "Bỏ qua lỗi, giữ phần thành công" }; policy.SelectedIndex = 0;
            var remember = new CheckBox { Content = "Lưu ánh xạ trong dự án Revit", IsChecked = true, Margin = new Thickness(0,16,0,8) };
            var fittings = new CheckBox { Content = "Nối đầu ống: elbow, transition, tee", Margin = new Thickness(0,8,0,8) };
            var terminals = new CheckBox { Content = "Nối miệng gió trong model chính tại đầu ống (tối đa 1 mm)", Margin = new Thickness(0,8,0,8) };
            foreach(var option in new[] { manualLevel, remember, fittings, terminals })
                option.Content=IFCInfoWindow.Text((string)option.Content,13,"#37322B");
            body.Children.Add(remember); body.Children.Add(fittings); body.Children.Add(terminals);
            body.Children.Add(IFCInfoWindow.Text("Khoảng hở tối đa cho elbow/transition (mm). Để 1 nếu chỉ nối các đầu gặp nhau; tăng nếu cho phép Revit điều chỉnh đầu ống.",13,"#716B63"));
            var gap=new TextBox { Text="1",Width=100,HorizontalAlignment=HorizontalAlignment.Left,Margin=new Thickness(0,8,0,8) }; body.Children.Add(gap);
            var preview = new DataGrid { ItemsSource = items, AutoGenerateColumns = false, CanUserAddRows = false, CanUserDeleteRows = false, Height = 190, Margin = new Thickness(0,16,0,8) };
            preview.Columns.Add(new DataGridCheckBoxColumn { Header = "Thực hiện", Binding = new System.Windows.Data.Binding("Include")
                { Mode = System.Windows.Data.BindingMode.TwoWay, UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged } });
            preview.Columns.Add(new DataGridTextColumn { Header = "Nguồn IFC", Binding = new System.Windows.Data.Binding("PreviewSource"), IsReadOnly=true });
            preview.Columns.Add(new DataGridTextColumn { Header = "Shape", Binding = new System.Windows.Data.Binding("ShapeName"), IsReadOnly=true });
            preview.Columns.Add(new DataGridTextColumn { Header = "System Type IFC", Binding = new System.Windows.Data.Binding("PreviewSystem"), IsReadOnly=true });
            preview.Columns.Add(new DataGridTextColumn { Header = "Duct đích", Binding = new System.Windows.Data.Binding("PreviewTarget"), IsReadOnly=true });
            preview.Columns.Add(new DataGridTextColumn { Header = "Thay đổi", Binding = new System.Windows.Data.Binding("PreviewChange"), IsReadOnly=true });
            UiDesign.StyleTable(preview);
            groupsBody.Children.Add(preview);
            Action refreshSelection = () =>
            {
                int count = items.Count(i => i.Include);
                create.Content = (updating ? "Cập nhật " : "Tạo ") + count + " Duct";
                create.IsEnabled = count > 0;
                ((TextBlock)summary.Child).Text = "Đã chọn: " + count + " đoạn     ·     Không chọn: "
                    + (items.Count - count) + " đoạn     ·     Bỏ qua: " + issues.Count + " đoạn";
            };
            System.ComponentModel.PropertyChangedEventHandler selectionChanged = (s, e) =>
            {
                if (e.PropertyName == nameof(DuctPlanItem.Include)) refreshSelection();
            };
            foreach (var item in items) item.PropertyChanged += selectionChanged;
            Closed += (s, e) => { foreach (var item in items) item.PropertyChanged -= selectionChanged; };
            refreshSelection();
            var note = IFCInfoWindow.Text("Fitting dùng Routing Preferences. Chỉ nối cặp đầu ống có nghiệm duy nhất; tee cần 3 đầu gặp nhau, không tự chia ống. System Name IFC lưu trong Comments.", 13, "#716B63");
            note.Margin = new Thickness(2,16,2,8); body.Children.Add(note);
            if (issues.Count > 0)
            {
                groupsBody.Children.Add(new SkippedDuctsPanel(issues));
                var exportIssues=IFCInfoWindow.Button("Xuất CSV các nguồn bị bỏ qua",false);
                exportIssues.Click+=(s,e)=>
                {
                    try
                    {
                        var file=new Microsoft.Win32.SaveFileDialog { Filter="CSV (*.csv)|*.csv",FileName="IFC-skipped.csv" };
                        if (file.ShowDialog(this)==true) DuctRunRow.Export(file.FileName,issues.Select(issue=>new DuctRunRow { SourceId=issue.Split(':')[0],Status="Bỏ qua",Reason=issue }));
                    }
                    catch (Exception ex) { feedback.Text=ex.Message; }
                };
                groupsBody.Children.Add(exportIssues);
            }
            create.Click += (s, e) =>
            {
                preview.CommitEdit(DataGridEditingUnit.Cell, true); preview.CommitEdit(DataGridEditingUnit.Row, true);
                double gapMm;
                if (!double.TryParse(gap.Text,out gapMm) || double.IsNaN(gapMm) || double.IsInfinity(gapMm) || gapMm<1 || gapMm>1000)
                { feedback.Text="Khoảng hở fitting phải từ 1 đến 1000 mm."; return; }
                var selected = items.Where(i => i.Include).ToList();
                if (selected.Count == 0) { feedback.Text = "Chọn ít nhất một dòng trong bảng xem trước."; return; }
                bool needsRound = selected.Any(i => i.Round), needsRectangular = selected.Any(i => !i.Round && !i.Oval);
                var systemKeys = new HashSet<string>(selected.Select(i => i.GroupKey));
                var levelKeys = new HashSet<string>(selected.Select(i => i.LevelKey ?? "Không có Level nguồn"));
                foreach (var key in systemKeys)
                {
                    string error = editors[key].Validate();
                    if (error != null) { feedback.Text = error; groupSearch.Text=""; editors[key].IsExpanded = true; editors[key].BringIntoView(); return; }
                }
                var newSystems = editors.Where(p => systemKeys.Contains(p.Key) && p.Value.NewSystem != null)
                    .ToDictionary(p => p.Key, p => p.Value.NewSystem);
                foreach (var group in newSystems.Values.GroupBy(n => n.Name, StringComparer.OrdinalIgnoreCase))
                    if (group.Select(n => n.TemplateId + "|" + (n.TemplateId > 0 ? "" : n.Classification)).Distinct().Count() > 1)
                    { feedback.Text = "System Type mới '" + group.Key + "' có thiết lập khác nhau giữa các nhóm. Chọn cùng mẫu/phân loại hoặc đổi tên."; return; }
                Request = new DuctRequest
                {
                    Items = selected,
                    RoundTypeId = needsRound ? ((DuctChoice)editors[selected.First(i => i.Round).GroupKey].TypeBox.SelectedItem).Id : 0,
                    RectangularTypeId = needsRectangular ? ((DuctChoice)editors[selected.First(i => !i.Round && !i.Oval).GroupKey].TypeBox.SelectedItem).Id : 0,
                    DuctTypes = editors.Where(p => systemKeys.Contains(p.Key)).ToDictionary(p => p.Key, p => ((DuctChoice)p.Value.TypeBox.SelectedItem).Id),
                    SystemTypes = editors.Where(p => systemKeys.Contains(p.Key)).ToDictionary(p => p.Key, p => p.Value.NewSystem != null ? 0 : ((DuctChoice)p.Value.SystemBox.SelectedItem).Id),
                    NewSystems = newSystems,
                    Levels = levelMapping.Where(p => levelKeys.Contains(p.Key)).ToDictionary(p => p.Key, p => manualLevel.IsChecked==true ? ((DuctChoice)p.Value.SelectedItem).Id : 0),
                    Worksets = worksetMapping.Where(p => systemKeys.Contains(p.Key)).ToDictionary(p => p.Key, p => ((DuctChoice)p.Value.SelectedItem).Id),
                    KeepSuccessful = policy.SelectedIndex == 1, SaveSettings = remember.IsChecked == true,
                    CreateFittings = fittings.IsChecked == true, ConnectTerminals = terminals.IsChecked == true,
                    FittingGapMm=gapMm,
                    Skipped = issues.Select(issue=>new DuctRunRow { SourceId=issue.Split(':')[0], Status="Bỏ qua", Reason=issue }).ToList()
                };
                Request.Skipped.AddRange(items.Where(i=>!i.Include).Select(i=>new DuctRunRow { SourceId=i.Source.ElementId,IfcGuid=i.Source.IfcGuid,Status="Không chọn",Reason="Bỏ chọn trong bảng xem trước." }));
                DialogResult = true;
            };
        }
    }
}




