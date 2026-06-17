using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ForexTradingWorkspace.Models.Learning;
using ForexTradingWorkspace.Services.Learning;

namespace ForexTradingWorkspace.ViewModels;

public partial class LessonEditorViewModel(ILessonRepository lessonRepository) : ObservableObject
{
    // ── left panel: lesson list ─────────────────────────────────────────────
    public ObservableCollection<LessonEditorItem> Items { get; } = [];

    [ObservableProperty] private LessonEditorItem? selectedItem;

    // ── right panel: edit fields ────────────────────────────────────────────
    [ObservableProperty] private string editSection = "";
    [ObservableProperty] private string editId = "";
    [ObservableProperty] private string editTitle = "";
    [ObservableProperty] private string editDuration = "";
    [ObservableProperty] private string editContent = "";
    [ObservableProperty] private string editTerms = "";
    [ObservableProperty] private string editPractice = "";
    [ObservableProperty] private string editMistakes = "";

    [ObservableProperty] private string status = "Load lessons to begin editing.";
    [ObservableProperty] private bool isDirty;
    public Visibility DirtyVisibility => IsDirty ? Visibility.Visible : Visibility.Collapsed;

    private List<LessonSection> _sections = [];

    public async Task InitializeAsync()
    {
        _sections = (await lessonRepository.LoadSectionsAsync()).ToList();
        RebuildList();
        SelectedItem = Items.FirstOrDefault();
        Status = $"Loaded {Items.Count} lessons.";
    }

    partial void OnSelectedItemChanged(LessonEditorItem? value)
    {
        if (value is null) return;
        var lesson = value.Lesson;
        EditSection  = value.SectionTitle;
        EditId       = lesson.Id;
        EditTitle    = lesson.Title;
        EditDuration = lesson.Duration;
        EditContent  = lesson.Summary.StartsWith("Study ", StringComparison.OrdinalIgnoreCase) ? "" : lesson.Summary;
        EditTerms    = string.Join(", ", lesson.KeyTerms);
        EditPractice = lesson.PracticeTasks.FirstOrDefault()?.Prompt ?? "";
        EditMistakes = string.Join(", ", lesson.CommonMistakes);
        IsDirty = false;
    }

    partial void OnEditContentChanged(string value)   => IsDirty = true;
    partial void OnEditTermsChanged(string value)     => IsDirty = true;
    partial void OnEditPracticeChanged(string value)  => IsDirty = true;
    partial void OnEditMistakesChanged(string value)  => IsDirty = true;
    partial void OnEditTitleChanged(string value)     => IsDirty = true;
    partial void OnEditDurationChanged(string value)  => IsDirty = true;
    partial void OnEditSectionChanged(string value)   => IsDirty = true;
    partial void OnIsDirtyChanged(bool value)         => OnPropertyChanged(nameof(DirtyVisibility));

    [RelayCommand]
    private async Task SaveCurrentAsync()
    {
        if (SelectedItem is null) return;

        var lesson = SelectedItem.Lesson;
        lesson.Title    = EditTitle.Trim();
        lesson.Duration = EditDuration.Trim();

        lesson.Summary = string.IsNullOrWhiteSpace(EditContent)
            ? $"Study {lesson.Title}, write the idea in your own words, then apply it to one low-risk XM demo plan."
            : EditContent.Trim();

        lesson.KeyTerms = ParseList(EditTerms);

        lesson.CommonMistakes = ParseList(EditMistakes);

        var practicePrompt = string.IsNullOrWhiteSpace(EditPractice)
            ? $"Explain how {lesson.Title} applies to a demo trade before execution."
            : EditPractice.Trim();

        lesson.PracticeTasks = [new PracticeTask
        {
            Id = $"{lesson.Id}-practice",
            Prompt = practicePrompt,
            ExpectedAction = "Write a short answer, then apply the concept in the trade planner."
        }];

        // move section if changed
        var newSection = EditSection.Trim();
        if (!string.Equals(SelectedItem.SectionTitle, newSection, StringComparison.OrdinalIgnoreCase))
        {
            MoveLessonToSection(lesson, SelectedItem.SectionTitle, newSection);
            SelectedItem.SectionTitle = newSection;
        }

        await SaveToFileAsync();
        IsDirty = false;
        Status = $"Saved: {lesson.Id} {lesson.Title}";
        RebuildList();
    }

    [RelayCommand]
    private async Task AddLessonAsync()
    {
        var section = string.IsNullOrWhiteSpace(EditSection) ? "My Lessons" : EditSection.Trim();
        var target  = _sections.FirstOrDefault(s => s.Title.Equals(section, StringComparison.OrdinalIgnoreCase));
        if (target is null)
        {
            target = new LessonSection
            {
                SectionNumber = _sections.Count + 2,
                Title = section,
                Lessons = []
            };
            _sections.Add(target);
        }

        var num    = target.Lessons.Count + 1;
        var newId  = $"{target.SectionNumber}.{num}";
        var lesson = new Lesson
        {
            Id            = newId,
            SectionNumber = target.SectionNumber,
            SectionTitle  = target.Title,
            Title         = "New Lesson",
            Duration      = "",
            Summary       = "",
            KeyTerms      = [],
            CommonMistakes = [],
            PracticeTasks = [new PracticeTask { Id = $"{newId}-practice", Prompt = "", ExpectedAction = "" }],
            DemoApplication = "",
            Checklist     = []
        };
        target.Lessons.Add(lesson);

        await SaveToFileAsync();
        RebuildList();
        SelectedItem = Items.FirstOrDefault(x => x.Lesson == lesson);
        Status = $"New lesson added to {section}. Fill in the details and save.";
    }

    [RelayCommand]
    private async Task DeleteCurrentAsync()
    {
        if (SelectedItem is null) return;
        var lesson  = SelectedItem.Lesson;
        var section = _sections.FirstOrDefault(s => s.Lessons.Contains(lesson));
        if (section is null) return;

        section.Lessons.Remove(lesson);
        if (section.Lessons.Count == 0)
            _sections.Remove(section);

        await SaveToFileAsync();
        RebuildList();
        SelectedItem = Items.FirstOrDefault();
        Status = $"Deleted: {lesson.Id} {lesson.Title}";
    }

    [RelayCommand]
    private async Task ReloadAsync()
    {
        _sections = (await lessonRepository.LoadSectionsAsync()).ToList();
        RebuildList();
        SelectedItem = Items.FirstOrDefault();
        Status = $"Reloaded {Items.Count} lessons from file.";
    }

    // ── helpers ─────────────────────────────────────────────────────────────

    private void RebuildList()
    {
        var previously = SelectedItem?.Lesson;
        Items.Clear();
        foreach (var section in _sections)
            foreach (var lesson in section.Lessons)
                Items.Add(new LessonEditorItem(lesson, section.Title));

        if (previously is not null)
            SelectedItem = Items.FirstOrDefault(x => x.Lesson == previously) ?? Items.FirstOrDefault();
    }

    private void MoveLessonToSection(Lesson lesson, string fromTitle, string toTitle)
    {
        var from = _sections.FirstOrDefault(s => s.Title.Equals(fromTitle, StringComparison.OrdinalIgnoreCase));
        from?.Lessons.Remove(lesson);

        var to = _sections.FirstOrDefault(s => s.Title.Equals(toTitle, StringComparison.OrdinalIgnoreCase));
        if (to is null)
        {
            to = new LessonSection { SectionNumber = _sections.Count + 2, Title = toTitle, Lessons = [] };
            _sections.Add(to);
        }
        lesson.SectionNumber = to.SectionNumber;
        lesson.SectionTitle  = to.Title;
        to.Lessons.Add(lesson);

        if (from?.Lessons.Count == 0) _sections.Remove(from);
    }

    private static List<string> ParseList(string raw) =>
        [.. raw.Split([',', ';', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
               .Where(x => !string.IsNullOrWhiteSpace(x))];

    private static readonly string LessonsFile =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                     "ForexTradingWorkspace", "Data", "lessons.txt");

    private async Task SaveToFileAsync()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("# ============================================================");
        sb.AppendLine("# MY FOREX LEARNING PLAN — lessons.txt");
        sb.AppendLine("# Edit in this app: Learn → Edit Lessons");
        sb.AppendLine("# ============================================================");
        sb.AppendLine();

        foreach (var section in _sections)
        {
            sb.AppendLine($"SECTION: {section.Title}");
            sb.AppendLine();

            foreach (var lesson in section.Lessons)
            {
                sb.AppendLine($"LESSON: {lesson.Id} {lesson.Title}");
                if (!string.IsNullOrWhiteSpace(lesson.Duration))
                    sb.AppendLine($"DURATION: {lesson.Duration}");

                var isGeneric = lesson.Summary.StartsWith("Study ", StringComparison.OrdinalIgnoreCase);
                if (!isGeneric && !string.IsNullOrWhiteSpace(lesson.Summary))
                {
                    sb.AppendLine("CONTENT:");
                    foreach (var line in lesson.Summary.Split('\n'))
                        sb.AppendLine(line.TrimEnd());
                }

                if (lesson.KeyTerms.Count > 0)
                    sb.AppendLine($"TERMS: {string.Join(", ", lesson.KeyTerms)}");

                var practice = lesson.PracticeTasks.FirstOrDefault()?.Prompt;
                if (!string.IsNullOrWhiteSpace(practice))
                    sb.AppendLine($"PRACTICE: {practice}");

                if (lesson.CommonMistakes.Count > 0)
                    sb.AppendLine($"MISTAKES: {string.Join(", ", lesson.CommonMistakes)}");

                sb.AppendLine("---");
                sb.AppendLine();
            }
        }

        Directory.CreateDirectory(Path.GetDirectoryName(LessonsFile)!);
        await File.WriteAllTextAsync(LessonsFile, sb.ToString());
    }
}

public sealed class LessonEditorItem(Lesson lesson, string sectionTitle) : ObservableObject
{
    public Lesson Lesson { get; } = lesson;

    [System.ComponentModel.Description("Section")]
    private string _sectionTitle = sectionTitle;
    public string SectionTitle
    {
        get => _sectionTitle;
        set => SetProperty(ref _sectionTitle, value);
    }

    public string DisplayLabel => $"{Lesson.Id}  {Lesson.Title}";
}
