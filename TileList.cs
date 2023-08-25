using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.Windows.Forms;



namespace TileList
{
    public class ModeConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(new[] { "Fill", "Manual" });
        }
    }
    public class TileListItemEventArgs : EventArgs
    {
        internal TileListItem Item { get; private set; }

        internal TileListItemEventArgs(TileListItem item)
        {
            Item = item;
        }
    }

    // Custom control for displaying TileListitems
    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(TileListItem))]
    public class TileListItem : Panel
    {
        // Event for when an item is double-clicked
        public event EventHandler<TileListItemEventArgs> OnItemClicked;
        public event EventHandler<TileListItemEventArgs> OnItemDoubleClicked;
        public event EventHandler ItemsChanges;

        public void On_Select(object sender, EventArgs e)
        {
            if (this.Selected == true)
            {
                Select(false);
            }
            else
            {
                Select(true);
            }
            OnItemClicked?.Invoke(this, new TileListItemEventArgs(this));
        }
        public void Select(bool selected)
        {
            if (selected == true)
            {
                Color parentColor = this.Parent.BackColor;
                pictureBox.Image = AdjustImageBrightness(pictureBox.Image, -0.08f);
                this.BackColor = ControlPaint.Light(parentColor, 9);
                this.Selected = true;
            }
            else
            {
                pictureBox.Image = AdjustImageBrightness(pictureBox.Image, 0.08f);
                this.BackColor = this.Parent.BackColor;
                this.Selected = false;
            }
        }

        // Event handler for when the mouse enters an item
        private void Item_MouseEnter(object sender, EventArgs e)
        {
            if (this.Parent != null)
            {
                Color parentColor = this.Parent.BackColor;
                if (this.Selected == true)
                {
                    this.BackColor = ControlPaint.Light(parentColor, 8);
                }
                else
                {
                    this.BackColor = ControlPaint.Light(parentColor, 9);
                }
                
                
                this.Cursor = Cursors.Hand;
            }
        }

        // Event handler for when the mouse leaves an item
        private void Item_MouseLeave(object sender, EventArgs e)
        {
            if (this.Parent != null)
            {
                if (this.Selected == false)
                {
                    this.BackColor = this.Parent.BackColor;
                }
                else
                {
                    Color parentColor = this.Parent.BackColor;
                    this.BackColor = ControlPaint.Light(parentColor, 9);
                }

            }
            this.Cursor = Cursors.Default;
        }

        // Public properties
        [Browsable(true)]
        [Category("Tile")]
        public override string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                if (!string.IsNullOrEmpty(_text))
                {
                    label.Text = _text;
                }
            }
        }

        [Browsable(true)]
        [Category("Tile")]
        public Image Picture
        {
            get { return _image; }
            set
            {
                _image = value;
                if (_image != null)
                {
                    pictureBox.Image = _image;
                }
            }
        }        
        [Browsable(true)]
        [Category("Tile")]
        public int Picture_Size
        {
            get { return _picture_size; }
            set
            {
                if (Picture_SizeMode == "Fill")
                {
                    pictureBox.Width = (int)(0.8 * pictureBox.Parent.Width);
                    pictureBox.Height = (int)(0.8 * pictureBox.Parent.Height);
                }
                else
                {
                    pictureBox.Width = _picture_size;
                    pictureBox.Height = _picture_size;
                }
                _picture_size = value;
            }
        }

        [Browsable(true)]
        [Category("Tile")]
        [TypeConverter(typeof(ModeConverter))]
        public string Picture_SizeMode { get; set; }

        [Browsable(true)]
        [Category("Tile")]
        public new int Width 
        {
            get { return _width; } 
            set
            {
                if (Picture_SizeMode == "Fill")
                {
                    pictureBox.Width = (int)(0.8 * pictureBox.Parent.Width);
                    pictureBox.Height = (int)(0.8 * pictureBox.Parent.Height);
                }
                else
                {
                    pictureBox.Width = _picture_size;
                    pictureBox.Height = _picture_size;
                }
                _width = value;
                base.Width = value;
            }

        }

        [Browsable(true)]
        [Category("Tile")]
        public new int Height
        {
            get { return _height; }
            set
            {
                if (Picture_SizeMode == "Fill")
                {
                    pictureBox.Width = (int)(0.8 * pictureBox.Parent.Width);
                    pictureBox.Height = (int)(0.8 * pictureBox.Parent.Height);
                }
                else
                {
                    pictureBox.Width = _picture_size;
                    pictureBox.Height = _picture_size;
                }
                _height = value;
                base.Height = value;
            }
        }
        // Private fields
        private Label label { get; set; }
        private PictureBox pictureBox { get; set; }
        private string _text;
        private Image _image;
        private int _picture_size { get; set; }
        private int _width { get; set; }
        private int _height { get; set; }
        internal bool Selected = false;
        internal int Id = 0;

        // Constructor
        public TileListItem()
        {
            //Create Context Menu
            this.ContextMenuStrip = new ContextMenuStrip();

            // Create the body of the TileListItem (add text and image)
            label = new Label();
            pictureBox = new PictureBox();
            Picture_SizeMode = "Fill";

            label.TextAlign = ContentAlignment.MiddleCenter;
            label.AutoSize = false;
            label.Dock = DockStyle.Bottom;

            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox.Dock = DockStyle.Top;
            pictureBox.Width = _picture_size;
            pictureBox.Height = _picture_size;

            this.Controls.Add(label);
            this.Controls.Add(pictureBox);

            this.MouseEnter += new EventHandler(Item_MouseEnter);
            label.MouseEnter += new EventHandler(Item_MouseEnter);
            pictureBox.MouseEnter += new EventHandler(Item_MouseEnter);
            this.MouseLeave += new EventHandler(Item_MouseLeave);
            label.MouseLeave += new EventHandler(Item_MouseLeave);
            pictureBox.MouseLeave += new EventHandler(Item_MouseLeave);

            this.MouseClick += On_Select;
            label.MouseClick += On_Select;
            pictureBox.MouseClick += On_Select;

            this.DoubleClick += _DoubleClick;
            label.DoubleClick += _DoubleClick;
            pictureBox.DoubleClick += _DoubleClick;

            this.Picture = TileList.Properties.Resource1.picture as Image;
        }

        private void _DoubleClick(object sender, EventArgs e)
        {
            OnItemDoubleClicked?.Invoke(this, new TileListItemEventArgs(this));
        }

        private Bitmap AdjustImageBrightness(Image image, float brightness)
        {
            Bitmap bmp = new Bitmap(image.Width, image.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                float[][] matrix = {
            new float[] {1, 0, 0, 0, 0},
            new float[] {0, 1, 0, 0, 0},
            new float[] {0, 0, 1, 0, 0},
            new float[] {0, 0, 0, 1, 0},
            new float[] {brightness, brightness, brightness, 0, 1}
        };
                ColorMatrix colorMatrix = new ColorMatrix(matrix);
                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(colorMatrix);
                g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height),
                    0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
            }
            return bmp;
        }
    }

    // Custom collection class for managing  items
    public class TileListCollection : Collection<TileListItem>
    {
        // Event for when an item in the collection changes
        public event EventHandler ItemChanged;
        public event EventHandler InternalItemChanged;

        // Event for when an item in the collection is double-clicked
        public event EventHandler<TileListItemEventArgs> ItemDoubleClicked;
        public event EventHandler<TileListItemEventArgs> ItemClicked;

        // Internal fields and properties
        private Collection<TileListItem> _items = new Collection<TileListItem>();

        public new int Count => _items.Count;

        public object SyncRoot => ((ICollection)_items).SyncRoot;

        public bool IsSynchronized => ((ICollection)_items).IsSynchronized;

        // Public methods
        public void Add(TileListItem item)
        {
            item.OnItemDoubleClicked += Item_DoubleClicked;
            item.OnItemClicked += Item_Clicked;
            item.ItemsChanges += Item_ChangedFromItem;
            _items.Add(item);
            ItemChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Remove(int index)
        {
            _items.RemoveAt(index);
            ItemChanged?.Invoke(this, EventArgs.Empty);
        }

        public new void Clear()
        {
            _items.Clear();
            ItemChanged?.Invoke(this, EventArgs.Empty);
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection)_items).CopyTo(array, index);
        }

        public new IEnumerator GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        // Private methods
        private void Item_DoubleClicked(object sender, TileListItemEventArgs e)
        {
            TileListItem item = e.Item;
            ItemDoubleClicked?.Invoke(this, e);
        }
        private void Item_Clicked(object sender, TileListItemEventArgs e)
        {
            TileListItem item = e.Item;
            ItemClicked?.Invoke(this, e);
        }
        private void Item_ChangedFromItem(object sender, EventArgs e)
        {
            InternalItemChanged?.Invoke(this, EventArgs.Empty);
        }
    }


    // Custom control for displaying a list of items
    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(TileListView))]
    public class TileListView : Panel
    {
        // Event for when an item in the list is double-clicked
        public event EventHandler<TileListItemEventArgs> ItemClicked;
        public event EventHandler<TileListItemEventArgs> ItemDoubleClicked;
        public event EventHandler ItemsChanged;
        public event EventHandler ItemsSizeChanged;

        // Event handler for when an item in the collection changes
        private void HandleItemChanged(object sender, EventArgs e)
        {
            if (this.Width > 0 && this.ReadyWrite == true)
            {
                DrawList(true);
            }
        }
        private void HandleItemsInternalChanged(object sender, EventArgs e)
        {
        }
        private void HandleListViewClick(object sender, EventArgs e)
        {
            foreach (TileListItem foritem in this.Items)
            {
                if (foritem.Selected == true)
                {
                    foritem.Select(false);
                }
            }
        }

        // Event handler for when the control's size changes
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            DrawList(false);
        }
        // Event handler for when the control's sgot an item clicked
        private void HandleItemDoubleClicked(object sender, TileListItemEventArgs e)
        {
            TileListItem item = e.Item;
            ItemDoubleClicked?.Invoke(this, new TileListItemEventArgs(item));
        }
        private void HandleItemClicked(object sender, TileListItemEventArgs e)
        {
            TileListItem item = e.Item;
            if (this.Multiple_Select == false)
            {
                if (item.Id != LastSelectedId)
                {
                    foreach (TileListItem foritem in this.Items)
                    {
                        if (foritem.Id == LastSelectedId)
                        {
                            if (foritem.Selected == true)
                                foritem.Select(false);
                        }
                        if (item.Id == foritem.Id)
                        {
                            if (foritem.Selected == false)
                                foritem.Select(true);
                        }
                    }
                }
            }
            else
            {
                if (item.Id != LastSelectedId)
                {
                    if (!System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl) && !System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.RightCtrl))
                    {
                        foreach (TileListItem foritem in this.Items)
                        {
                            if (item.Id == foritem.Id)
                            {
                                if (foritem.Selected == false)
                                    foritem.Select(true);
                            }
                            else
                            {
                                if (foritem.Selected == true)
                                    foritem.Select(false);
                            }
                        }
                    }

                    if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftShift) || System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.RightShift))
                    {
                        foreach (TileListItem foritem in this.Items)
                        {
                            if (item.Id < LastSelectedId)
                            {
                                if (foritem.Id >= item.Id)
                                {
                                    if (foritem.Id <= LastSelectedId)
                                    {
                                        if (foritem.Selected == false)
                                        {
                                            foritem.Select(true);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (foritem.Id >= LastSelectedId)
                                {
                                    if (foritem.Id <= item.Id)
                                    {
                                        if (foritem.Selected == false)
                                        {
                                            foritem.Select(true);
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
            }
            LastSelectedId = item.Id;
            ItemClicked?.Invoke(this, new TileListItemEventArgs(item));
        }

        // Internal fields and properties
        private bool ReadyWrite = false;
        private int RowX { get; set; }
        private int RowY { get; set; }
        private int RowCount { get; set; }
        private int RowMaxCount { get; set; }
        private int _height { get; set; }
        private int _width { get; set; }
        private int _picture_size { get; set; }
        private int _Margin { get; set; }
        private string _mode { get; set; }

        private int HeightMargin = 2;
        private int LastSelectedId = 0;

        // Public fields and properties
        [Category("Data")]
        public TileListCollection Items { get; set; }

        [Browsable(true)]
        [Category("Tile")]
        public new int Height { get { return _height; } set { HandleItemChanged(this, EventArgs.Empty); _height = value; } }
        [Browsable(true)]
        [Category("Tile")]
        public new int Width { get { return _width; } set{ HandleItemChanged(this, EventArgs.Empty); _width = value;} }
        [Browsable(true)]
        [Category("Tile")]
        public int Picture_Size { get { return _picture_size; } set { ResizeItems(this, EventArgs.Empty); _picture_size = value; } }
        [Browsable(true)]
        [Category("Tile")]
        public new int Margin { get { return _Margin; } set { HandleItemChanged(this, EventArgs.Empty); _Margin = value; } }
        [Browsable(true)]
        [Category("Tile")]
        [TypeConverter(typeof(ModeConverter))]
        public string Picture_SizeMode
        {
            get { return _mode; }
            set { ResizeItems(this, EventArgs.Empty); _mode = value; }
        }
        [Browsable(true)]
        [Category("Tile")]
        [TypeConverter(typeof(bool))]
        public bool Multiple_Select { get; set; }

        // Constructor
        public TileListView()
        {
            Width = 85;
            Height = 95;
            Picture_Size = 50;
            Picture_SizeMode = "Fill";
            Margin = 15;

            Items = new TileListCollection();
            Items.InternalItemChanged += HandleItemsInternalChanged;
            Items.ItemChanged += HandleItemChanged;
            Items.ItemDoubleClicked += HandleItemDoubleClicked;
            Items.ItemClicked += HandleItemClicked;
            this.ItemsSizeChanged += ResizeItems;
            this.AutoScroll = true;
            this.HorizontalScroll.Enabled = false;
            this.ReadyWrite = true;
            Multiple_Select = false;
            this.Click += HandleListViewClick;
        }

        // Internal functions
        private void ResizeItems(object sender, EventArgs e)
        {
            if (this.Width > 0 && this.ReadyWrite == true)
            {
                foreach (TileListItem item in this.Items)
                {
                    item.Picture_Size = Picture_Size;
                    item.Picture_SizeMode = Picture_SizeMode;
                }
            }
        }

        private void DrawList(bool must_be_draw)
        {
            int i = 0;
            HeightMargin = Margin / 2;
            this.ReadyWrite = false;
            if ((this.ClientSize.Width / Width) != RowMaxCount || must_be_draw == true)
            {
                RowX = 20;
                RowY = 20;
                RowCount = 0;
                this.Controls.Clear();
                foreach (TileListItem item in this.Items)
                {
                    RowMaxCount = this.ClientSize.Width / Width;

                    if (RowCount < RowMaxCount && this.ClientSize.Width - RowX >= Width + Margin) // Check if there is enough space for the next item
                    {
                        item.Location = new Point(RowX, RowY);
                        item.Height = this.Height;
                        item.Width = this.Width;
                        this.Controls.Add(item);
                        RowX += Width + Margin; // Add Margin to RowX
                        RowCount++;
                    }
                    else
                    {
                        RowX = 20;
                        RowY += Height + HeightMargin; // Add HeightMargin to RowY
                        RowCount = -1;
                        item.Location = new Point(RowX, RowY);
                        item.Size = new Size(Width, Height);
                        if(item.Selected == true)
                        item.Select(false);
                        this.Controls.Add(item);
                        RowCount++;
                    }
                    item.Id = i;
                    i++;
                }
                this.AutoScrollMinSize = new Size(0, RowY + (RowCount > 0 ? Height : 0));
            }
            this.ReadyWrite = true;
        }

    }
}
