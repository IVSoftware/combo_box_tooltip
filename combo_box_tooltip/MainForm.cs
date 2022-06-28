using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

// https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.combobox.drawmode?view=windowsdesktop-6.0
// https://stackoverflow.com/questions/33697593/winforms-disable-default-mouse-hover-over-item-behaviour
namespace combo_box_tooltip
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            
            // Works with or without owner draw.
            // This line may be commented out
            comboBox.DrawMode = DrawMode.OwnerDrawFixed;

            // Add items
            foreach (var item in Enum.GetValues(typeof(ComboBoxItems))) comboBox.Items.Add(item);
            // Start timer on drop down
            comboBox.DropDown += (sender, e) => _toolTipTimer.Enabled = true;
            // End timer on closed
            comboBox.DropDownClosed += (sender, e) =>
            {
                _toolTipTimer.Enabled = false; 
                _lastToolTipIndex = -1;
                _toolTip.Hide(comboBox);
            };
            // Poll index while open
            _toolTipTimer.Tick += (sender, e) =>
            {
                if(comboBox.SelectedIndex != _lastToolTipIndex)
                {
                    _lastToolTipIndex = comboBox.SelectedIndex;
                    showToolTip();
                }
            };
            void showToolTip()
            {
                if (_lastToolTipIndex != -1)
                {
                    // Get the item
                    var item = (ComboBoxItems)comboBox.Items[_lastToolTipIndex];
                    // Get the tip
                    var tt = TipAttribute.FromMember(item);
                    // Get the rel pos
                    var mousePosition = PointToClient(MousePosition);
                    var rel = new Point(
                        (mousePosition.X - comboBox.Location.X) - 10,
                        (mousePosition.Y - comboBox.Location.Y) - 30);
                    // Show the tip
                    _toolTip.Show(tt, comboBox, rel);
                }
            }
            // Owner Draw
            comboBox.DrawItem += (sender, e) =>
            {
                Color fruitColor = new Color();
                var item = (ComboBoxItems)comboBox.Items[e.Index];
                switch (item)
                {
                    case ComboBoxItems.Apple:
                        fruitColor = Color.Red;
                        break;
                    case ComboBoxItems.Orange:
                        fruitColor = Color.Orange;
                        break;
                    case ComboBoxItems.Grape:
                        fruitColor = Color.Purple;
                        break;
                }
                e.DrawBackground();
                Rectangle rectangle = new Rectangle(2, e.Bounds.Top + 2,
                        e.Bounds.Height, e.Bounds.Height - 4);
                e.Graphics.FillRectangle(new SolidBrush(fruitColor), rectangle);
                e.Graphics.DrawString(
                    comboBox.Items[e.Index].ToString(), 
                    comboBox.Font, 
                    Brushes.Black, 
                    new RectangleF(e.Bounds.X + rectangle.Width, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height));
                e.DrawFocusRectangle();
            };
        }
        private ToolTip _toolTip = new ToolTip();
        private readonly Timer _toolTipTimer = new Timer() { Interval = 100 };
        private int _lastToolTipIndex = -1;
    }
    enum ComboBoxItems
    {
        [Tip("...a day keeps the doctor away")]
        Apple,
        [Tip("...you glad I didn't say 'banana?'")]
        Orange,
        [Tip("...job on the Tool Tips!")]
        Grape,
    }

    internal class TipAttribute : Attribute
    {
        public TipAttribute(string tip) => Tip = tip;
        public string Tip { get; }
        public static string FromMember(Enum value) =>
            ((TipAttribute)value
                .GetType()
                .GetMember($"{value}")
                .Single()
                .GetCustomAttribute(typeof(TipAttribute))).Tip;
    }
}
