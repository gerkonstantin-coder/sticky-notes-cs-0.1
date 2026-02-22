// Sticky Notes - Нативное приложение для Windows
// Компиляция: csc /target:winexe /r:System.dll /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Runtime.Serialization.dll /win32icon:icon.ico StickyNotes.cs
// Требуется: .NET Framework 4.0+ (встроен в Windows 10/11)

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace StickyNotes
{
    /// <summary>
    /// Данные заметки
    /// </summary>
    [DataContract]
    public class NoteData
    {
        [DataMember] public string Id { get; set; } = Guid.NewGuid().ToString().Substring(0, 8);
        [DataMember] public string Text { get; set; } = "";
        [DataMember] public int X { get; set; } = 100;
        [DataMember] public int Y { get; set; } = 100;
        [DataMember] public int Width { get; set; } = 240;
        [DataMember] public int Height { get; set; } = 240;
        [DataMember] public bool AlwaysOnTop { get; set; } = false;
        [DataMember] public string Color { get; set; } = "yellow";
    }

    /// <summary>
    /// Данные всех заметок
    /// </summary>
    [DataContract]
    public class NotesStorage
    {
        [DataMember] public List<NoteData> Notes { get; set; } = new List<NoteData>();
    }

    /// <summary>
    /// Цветовая схема стикера
    /// </summary>
    public class ColorScheme
    {
        public Color BackColor { get; set; }
        public Color HeaderColor { get; set; }
        public Color TextColor { get; set; }
        public Color ButtonColor { get; set; }
        public Color ButtonActiveColor { get; set; }

        public static Dictionary<string, ColorScheme> Schemes = new Dictionary<string, ColorScheme>
        {
            {"yellow", new ColorScheme {
                BackColor = Color.FromArgb(255, 249, 196),
                HeaderColor = Color.FromArgb(255, 213, 79),
                TextColor = Color.FromArgb(62, 39, 35),
                ButtonColor = Color.FromArgb(255, 160, 0),
                ButtonActiveColor = Color.FromArgb(255, 111, 0)
            }},
            {"green", new ColorScheme {
                BackColor = Color.FromArgb(232, 245, 233),
                HeaderColor = Color.FromArgb(129, 199, 132),
                TextColor = Color.FromArgb(27, 94, 32),
                ButtonColor = Color.FromArgb(76, 175, 80),
                ButtonActiveColor = Color.FromArgb(46, 125, 50)
            }},
            {"blue", new ColorScheme {
                BackColor = Color.FromArgb(227, 242, 253),
                HeaderColor = Color.FromArgb(100, 181, 246),
                TextColor = Color.FromArgb(13, 71, 161),
                ButtonColor = Color.FromArgb(33, 150, 243),
                ButtonActiveColor = Color.FromArgb(21, 101, 192)
            }},
            {"pink", new ColorScheme {
                BackColor = Color.FromArgb(252, 228, 236),
                HeaderColor = Color.FromArgb(240, 98, 146),
                TextColor = Color.FromArgb(136, 14, 79),
                ButtonColor = Color.FromArgb(233, 30, 99),
                ButtonActiveColor = Color.FromArgb(173, 20, 87)
            }},
            {"orange", new ColorScheme {
                BackColor = Color.FromArgb(255, 243, 224),
                HeaderColor = Color.FromArgb(255, 183, 77),
                TextColor = Color.FromArgb(230, 81, 0),
                ButtonColor = Color.FromArgb(255, 152, 0),
                ButtonActiveColor = Color.FromArgb(239, 108, 0)
            }},
            {"purple", new ColorScheme {
                BackColor = Color.FromArgb(243, 229, 245),
                HeaderColor = Color.FromArgb(186, 104, 200),
                TextColor = Color.FromArgb(74, 20, 140),
                ButtonColor = Color.FromArgb(156, 39, 176),
                ButtonActiveColor = Color.FromArgb(106, 27, 154)
            }}
        };
    }

    /// <summary>
    /// Главная форма стикера
    /// </summary>
    public class StickyNoteForm : Form
    {
        // Windows API функции
        [DllImport("user32.dll")]
        private static extern IntPtr GetParent(IntPtr hWnd);
        
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        
        [DllImport("user32.dll")]
        private static extern bool SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_APPWINDOW = 0x00040000;
        private const int SWP_FRAMECHANGED = 0x0020;

        private NoteData data;
        private ColorScheme colorScheme;
        private Panel headerPanel;
        private Panel buttonsPanel;
        private TextBox textBox;
        private Timer hideButtonsTimer;
        private bool buttonsVisible = true;
        private Point dragStart;
        private bool isDragging = false;
        private Form mainForm;
        private Action<NoteData> onClose;
        private Action onCreateNew;

        public StickyNoteForm(NoteData data, Form mainForm, Action<NoteData> onClose, Action onCreateNew)
        {
            this.data = data;
            this.mainForm = mainForm;
            this.onClose = onClose;
            this.onCreateNew = onCreateNew;
            this.colorScheme = ColorScheme.Schemes.GetValueOrDefault(data.Color, ColorScheme.Schemes["yellow"]);

            InitializeForm();
            InitializeComponents();
            SetupEventHandlers();
            StartHideTimer();

            // Применяем сохраненное состояние
            if (data.AlwaysOnTop)
            {
                TopMost = true;
            }
        }

        private void InitializeForm()
        {
            // Основные настройки формы
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(data.X, data.Y);
            Size = new Size(data.Width, data.Height);
            MinimumSize = new Size(180, 150);
            BackColor = colorScheme.BackColor;
            ShowInTaskbar = true;
            Opacity = 0.98;

            // Тень окна
            if (Environment.OSVersion.Version >= new Version(6, 2)) // Windows 8+
            {
                // Доступно в более новых версиях
            }
        }

        private void InitializeComponents()
        {
            // Заголовок (для перетаскивания)
            headerPanel = new Panel
            {
                Height = 32,
                Dock = DockStyle.Top,
                BackColor = colorScheme.HeaderColor,
                Cursor = Cursors.SizeAll
            };
            Controls.Add(headerPanel);

            // Панель кнопок
            buttonsPanel = new Panel
            {
                Height = 28,
                Width = 140,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            buttonsPanel.Location = new Point(Width - buttonsPanel.Width - 4, 2);
            headerPanel.Controls.Add(buttonsPanel);

            // Кнопки
            CreateButtons();

            // Текстовое поле
            textBox = new TextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                BackColor = colorScheme.BackColor,
                ForeColor = colorScheme.TextColor,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 11f),
                Text = data.Text,
                Padding = new Padding(8),
                ScrollBars = ScrollBars.Vertical
            };
            Controls.Add(textBox);
            
            // Перемещаем заголовок на верх
            Controls.SetChildIndex(headerPanel, 0);

            // Уголок для изменения размера
            var resizeGrip = new Label
            {
                Size = new Size(16, 16),
                Cursor = Cursors.SizeNWSE,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                BackColor = Color.Transparent,
                Text = "▭",
                TextAlign = ContentAlignment.BottomRight,
                Font = new Font("Segoe UI", 8f),
                ForeColor = colorScheme.HeaderColor
            };
            resizeGrip.Location = new Point(Width - 18, Height - 18);
            Controls.Add(resizeGrip);

            // События изменения размера
            var resizeStart = Point.Empty;
            var startSize = Size.Empty;

            resizeGrip.MouseDown += (s, e) =>
            {
                resizeStart = Control.MousePosition;
                startSize = Size;
            };

            resizeGrip.MouseMove += (s, e) =>
            {
                if (e.Button == MouseButtons.Left && resizeStart != Point.Empty)
                {
                    var delta = Control.MousePosition.X - resizeStart.X;
                    var deltaY = Control.MousePosition.Y - resizeStart.Y;
                    var newWidth = Math.Max(MinimumSize.Width, startSize.Width + delta);
                    var newHeight = Math.Max(MinimumSize.Height, startSize.Height + deltaY);
                    Size = new Size(newWidth, newHeight);
                }
            };

            resizeGrip.MouseUp += (s, e) =>
            {
                data.Width = Width;
                data.Height = Height;
                SaveNote();
                resizeStart = Point.Empty;
            };
        }

        private void CreateButtons()
        {
            int x = 0;
            var buttonSize = new Size(26, 24);
            var font = new Font("Segoe UI", 10f);

            // Кнопка создания новой заметки
            var newBtn = CreateButton("➕", buttonSize, font, x);
            newBtn.Click += (s, e) => onCreateNew();
            newBtn.ToolTip("Новая заметка");
            buttonsPanel.Controls.Add(newBtn);
            x += 28;

            // Кнопка закрепления на рабочем столе (пока без функции)
            var pinBtn = CreateButton("📌", buttonSize, font, x);
            pinBtn.Click += (s, e) => ToggleDesktopMode();
            pinBtn.ToolTip("Закрепить на рабочем столе");
            buttonsPanel.Controls.Add(pinBtn);
            x += 28;

            // Кнопка поверх всех окон
            var topBtn = CreateButton("⬆", buttonSize, font, x);
            topBtn.Click += (s, e) => ToggleAlwaysOnTop();
            topBtn.ToolTip("Поверх всех окон");
            if (data.AlwaysOnTop) topBtn.BackColor = colorScheme.ButtonActiveColor;
            buttonsPanel.Controls.Add(topBtn);
            x += 28;

            // Кнопка выбора цвета
            var colorBtn = CreateButton("🎨", buttonSize, font, x);
            colorBtn.Click += (s, e) => ShowColorMenu(colorBtn);
            colorBtn.ToolTip("Изменить цвет");
            buttonsPanel.Controls.Add(colorBtn);
            x += 28;

            // Кнопка закрытия
            var closeBtn = CreateButton("✕", buttonSize, new Font("Segoe UI", 11f, FontStyle.Bold), x);
            closeBtn.BackColor = Color.FromArgb(229, 115, 115);
            closeBtn.Click += (s, e) => CloseNote();
            closeBtn.ToolTip("Закрыть");
            buttonsPanel.Controls.Add(closeBtn);
        }

        private Button CreateButton(string text, Size size, Font font, int x)
        {
            return new Button
            {
                Text = text,
                Size = size,
                Location = new Point(x, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = colorScheme.ButtonColor,
                ForeColor = Color.White,
                Font = font,
                Cursor = Cursors.Hand
            };
        }

        private void SetupEventHandlers()
        {
            // Перетаскивание
            headerPanel.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    isDragging = true;
                    dragStart = e.Location;
                    CancelHideTimer();
                }
            };

            headerPanel.MouseMove += (s, e) =>
            {
                if (isDragging)
                {
                    var newLocation = Point.Subtract(Cursor.Position, new Size(dragStart));
                    Location = newLocation;
                    data.X = newLocation.X;
                    data.Y = newLocation.Y;
                }
            };

            headerPanel.MouseUp += (s, e) =>
            {
                if (isDragging)
                {
                    isDragging = false;
                    SaveNote();
                    StartHideTimer();
                }
            };

            // Показ/скрытие кнопок
            headerPanel.MouseEnter += (s, e) =>
            {
                ShowButtons();
                CancelHideTimer();
            };

            headerPanel.MouseLeave += (s, e) =>
            {
                if (!buttonsPanel.Bounds.Contains(headerPanel.PointToClient(Cursor.Position)))
                {
                    StartHideTimer();
                }
            };

            buttonsPanel.MouseEnter += (s, e) => CancelHideTimer();
            buttonsPanel.MouseLeave += (s, e) => StartHideTimer();

            // Сохранение текста
            textBox.TextChanged += (s, e) =>
            {
                data.Text = textBox.Text;
                SaveNote();
            };

            // Двойной клик для сворачивания
            headerPanel.DoubleClick += (s, e) =>
            {
                if (textBox.Visible)
                {
                    textBox.Hide();
                    Height = headerPanel.Height;
                }
                else
                {
                    textBox.Show();
                    Height = data.Height;
                }
            };

            // Правый клик - меню
            headerPanel.ContextMenuStrip = CreateContextMenu();
            textBox.ContextMenuStrip = CreateContextMenu();

            // Закрытие формы
            FormClosing += (s, e) =>
            {
                SaveNote();
                onClose(data);
            };
        }

        private ContextMenuStrip CreateContextMenu()
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("➕ Новая заметка", null, (s, e) => onCreateNew());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("⬆ Поверх всех окон", null, (s, e) => ToggleAlwaysOnTop());
            menu.Items.Add(new ToolStripSeparator());

            // Подменю цветов
            var colorMenu = new ToolStripMenuItem("🎨 Цвет");
            foreach (var colorName in ColorScheme.Schemes.Keys)
            {
                var item = new ToolStripMenuItem(colorName.ToUpper()[0] + colorName.Substring(1));
                item.Click += (s, e) => ChangeColor(colorName);
                colorMenu.DropDownItems.Add(item);
            }
            menu.Items.Add(colorMenu);

            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("✕ Закрыть", null, (s, e) => CloseNote());

            return menu;
        }

        private void ShowButtons()
        {
            if (!buttonsVisible)
            {
                buttonsVisible = true;
                buttonsPanel.Show();
            }
        }

        private void HideButtons()
        {
            if (buttonsVisible && !buttonsPanel.Bounds.Contains(buttonsPanel.PointToClient(Cursor.Position)))
            {
                buttonsVisible = false;
                buttonsPanel.Hide();
            }
        }

        private void StartHideTimer()
        {
            CancelHideTimer();
            hideButtonsTimer = new Timer { Interval = 3000 };
            hideButtonsTimer.Tick += (s, e) =>
            {
                HideButtons();
                hideButtonsTimer.Stop();
            };
            hideButtonsTimer.Start();
        }

        private void CancelHideTimer()
        {
            if (hideButtonsTimer != null)
            {
                hideButtonsTimer.Stop();
                hideButtonsTimer.Dispose();
                hideButtonsTimer = null;
            }
        }

        private void ToggleAlwaysOnTop()
        {
            data.AlwaysOnTop = !data.AlwaysOnTop;
            TopMost = data.AlwaysOnTop;
            
            // Обновляем кнопку
            foreach (Control ctrl in buttonsPanel.Controls)
            {
                if (ctrl.Text == "⬆")
                {
                    ctrl.BackColor = data.AlwaysOnTop ? colorScheme.ButtonActiveColor : colorScheme.ButtonColor;
                    break;
                }
            }
            
            SaveNote();
        }

        private void ToggleDesktopMode()
        {
            try
            {
                IntPtr handle = this.Handle;
                
                if (!data.AlwaysOnTop)
                {
                    // Закрепляем на рабочем столе
                    IntPtr progman = FindWindow("Progman", "Program Manager");
                    if (progman != IntPtr.Zero)
                    {
                        SetParent(handle, progman);
                    }
                    
                    // Обновляем кнопку
                    foreach (Control ctrl in buttonsPanel.Controls)
                    {
                        if (ctrl.Text == "📌")
                        {
                            ctrl.BackColor = colorScheme.ButtonActiveColor;
                            break;
                        }
                    }
                    
                    data.AlwaysOnTop = true; // Используем как флаг режима
                }
                else
                {
                    // Восстанавливаем нормальный режим
                    SetParent(handle, IntPtr.Zero);
                    
                    int exStyle = GetWindowLong(handle, GWL_EXSTYLE);
                    exStyle &= ~WS_EX_TOOLWINDOW;
                    exStyle |= WS_EX_APPWINDOW;
                    SetWindowLong(handle, GWL_EXSTYLE, exStyle);
                    
                    // Обновляем кнопку
                    foreach (Control ctrl in buttonsPanel.Controls)
                    {
                        if (ctrl.Text == "📌")
                        {
                            ctrl.BackColor = colorScheme.ButtonColor;
                            break;
                        }
                    }
                    
                    data.AlwaysOnTop = false;
                }
                
                SaveNote();
            }
            catch
            {
                // Игнорируем ошибки
            }
        }

        private void ShowColorMenu(Button sender)
        {
            var menu = new ContextMenuStrip();
            foreach (var colorName in ColorScheme.Schemes.Keys)
            {
                var item = new ToolStripMenuItem(colorName.ToUpper()[0] + colorName.Substring(1));
                item.Click += (s, e) => ChangeColor(colorName);
                menu.Items.Add(item);
            }
            menu.Show(sender, new Point(0, sender.Height));
        }

        private void ChangeColor(string colorName)
        {
            if (!ColorScheme.Schemes.ContainsKey(colorName)) return;

            data.Color = colorName;
            colorScheme = ColorScheme.Schemes[colorName];

            // Обновляем цвета
            BackColor = colorScheme.BackColor;
            headerPanel.BackColor = colorScheme.HeaderColor;
            textBox.BackColor = colorScheme.BackColor;
            textBox.ForeColor = colorScheme.TextColor;

            // Обновляем кнопки
            foreach (Control ctrl in buttonsPanel.Controls)
            {
                if (ctrl.Text != "✕" && ctrl.Text != "⬆" && ctrl.Text != "📌")
                {
                    ctrl.BackColor = colorScheme.ButtonColor;
                }
            }

            SaveNote();
        }

        private void SaveNote()
        {
            data.Text = textBox.Text;
            mainForm?.Invoke((MethodInvoker)(() => { }));
        }

        private void CloseNote()
        {
            SaveNote();
            Close();
        }
    }

    /// <summary>
    /// Главная программа
    /// </summary>
    public class Program : Form
    {
        private List<StickyNoteForm> notes = new List<StickyNoteForm>();
        private NotesStorage storage = new NotesStorage();
        private string storagePath;

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Program());
        }

        public Program()
        {
            // Скрываем главную форму
            Opacity = 0;
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.None;
            Size = new Size(0, 0);

            // Путь к файлу данных
            storagePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "StickyNotes",
                "notes.json"
            );

            LoadNotes();
        }

        private void LoadNotes()
        {
            if (File.Exists(storagePath))
            {
                try
                {
                    var json = File.ReadAllText(storagePath);
                    var serializer = new DataContractJsonSerializer(typeof(NotesStorage));
                    using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                    {
                        storage = (NotesStorage)serializer.ReadObject(stream);
                    }
                }
                catch
                {
                    storage = new NotesStorage();
                }
            }

            if (storage.Notes.Count == 0)
            {
                storage.Notes.Add(new NoteData());
            }

            foreach (var noteData in storage.Notes)
            {
                CreateNote(noteData);
            }
        }

        private void CreateNote(NoteData data = null)
        {
            if (data == null)
            {
                data = new NoteData();
                if (notes.Count > 0)
                {
                    var lastNote = notes[notes.Count - 1];
                    data.X = lastNote.Location.X + 30;
                    data.Y = lastNote.Location.Y + 30;
                }
            }

            var note = new StickyNoteForm(data, this, OnNoteClosed, CreateNote);
            notes.Add(note);
            note.Show();
        }

        private void OnNoteClosed(NoteData data)
        {
            storage.Notes.RemoveAll(n => n.Id == data.Id);
            notes.RemoveAll(n => n.Data.Id == data.Id);
            SaveNotes();
        }

        private void SaveNotes()
        {
            storage.Notes.Clear();
            foreach (var note in notes)
            {
                storage.Notes.Add(note.Data);
            }

            try
            {
                var dir = Path.GetDirectoryName(storagePath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var serializer = new DataContractJsonSerializer(typeof(NotesStorage));
                using (var stream = new MemoryStream())
                {
                    serializer.WriteObject(stream, storage);
                    var json = Encoding.UTF8.GetString(stream.ToArray());
                    File.WriteAllText(storagePath, json);
                }
            }
            catch
            {
                // Игнорируем ошибки сохранения
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            SaveNotes();
            base.OnFormClosed(e);
        }
    }

    // Расширение для ToolTip
    public static class ControlExtensions
    {
        private static Dictionary<Control, ToolTip> tooltips = new Dictionary<Control, ToolTip>();

        public static void ToolTip(this Control control, string text)
        {
            if (!tooltips.ContainsKey(control))
            {
                tooltips[control] = new ToolTip
                {
                    InitialDelay = 500,
                    ShowAlways = true
                };
            }
            tooltips[control].SetToolTip(control, text);
        }
    }

    // Расширение для Dictionary
    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default)
        {
            return dict.ContainsKey(key) ? dict[key] : defaultValue;
        }
    }
}
