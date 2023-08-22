using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;


namespace TileList
{
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
        public event EventHandler<TileListItemEventArgs> OnItemDoubleClicked;
        public event EventHandler ItemsChanges;

        // Event handler for when the mouse enters an item
        private void Item_MouseEnter(object sender, EventArgs e)
        {
            if (this.Parent != null)
            {
                Color parentColor = this.Parent.BackColor;
                float fadeAmount = (float)6;
                this.BackColor = ControlPaint.Light(parentColor, fadeAmount);
                this.Cursor = Cursors.Hand;
            }
        }

        // Event handler for when the mouse leaves an item
        private void Item_MouseLeave(object sender, EventArgs e)
        {
            if (this.Parent != null)
            {
                this.BackColor = this.Parent.BackColor;
            }
            this.Cursor = Cursors.Default;
        }

        private void contextMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

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
        public int Picture_Height
        {
            get { return _picture_height; }
            set
            {
                int retval = Height;

                if (value <= (int)(Height/1.5))
                {
                    retval = value;
                }
                else
                {
                    retval = (int)(Height/1.5);
                }
                _picture_height = retval;
                pictureBox.Height = retval;
            }
        }
        [Browsable(true)]
        [Category("Tile")]
        public int Picture_Width
        {
            get { return _picture_width; }
            set
            {
                int retval = Width;

                if (value <= Width)
                {
                    retval = value;
                }
                else
                {
                    retval = Width;
                }
                _picture_width = retval;
                pictureBox.Width = retval;
            }
        }

        // Private fields
        private Label label { get; set; }
        private PictureBox pictureBox { get; set; }
        private string _text;
        private Image _image;
        private int _picture_width { get; set; }
        private int _picture_height { get; set; }

        // Constructor
        public TileListItem()
        {
            //Create Context Menu
            this.ContextMenuStrip = new ContextMenuStrip();

            // Create the body of the TileListItem (add text and image)
            label = new Label();
            pictureBox = new PictureBox();
            pictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBox.Dock = DockStyle.Fill;
            label.Dock = DockStyle.Fill;
            label.TextAlign = ContentAlignment.MiddleCenter;
            label.AutoSize = false;
            Panel labpan = new Panel();
            Panel picpan = new Panel();
            picpan.Height = pictureBox.Height;
            labpan.Controls.Add(label);
            picpan.Controls.Add(pictureBox);
            picpan.Dock = DockStyle.Top;
            labpan.Dock = DockStyle.Bottom;
            this.Controls.Add(picpan);
            this.Controls.Add(labpan);

            this.MouseEnter += new EventHandler(Item_MouseEnter);
            label.MouseEnter += new EventHandler(Item_MouseEnter);
            pictureBox.MouseEnter += new EventHandler(Item_MouseEnter);
            this.MouseLeave += new EventHandler(Item_MouseLeave);
            label.MouseLeave += new EventHandler(Item_MouseLeave);
            pictureBox.MouseLeave += new EventHandler(Item_MouseLeave);

        }

        private void _DoubleClick(object sender, EventArgs e)
        {
            OnItemDoubleClicked?.Invoke(this, new TileListItemEventArgs(this));
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

        // Internal fields and properties
        private Collection<TileListItem> _items = new Collection<TileListItem>();

        public new int Count => _items.Count;

        public object SyncRoot => ((ICollection)_items).SyncRoot;

        public bool IsSynchronized => ((ICollection)_items).IsSynchronized;

        // Public methods
        public void Add(TileListItem item)
        {
            item.OnItemDoubleClicked += Item_DoubleClicked;
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

        public IEnumerator GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        // Private methods
        private void Item_DoubleClicked(object sender, TileListItemEventArgs e)
        {
            TileListItem item = e.Item;
            ItemDoubleClicked?.Invoke(this, e);
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
        public event EventHandler<TileListItemEventArgs> ItemDoubleClicked;
        public event EventHandler ItemsChanged;
        public event EventHandler ItemsSizeChanged;

        // Event handler for when an item in the collection changes
        private void HandleItemChanged(object sender, EventArgs e)
        {
            DrawList(true);
        }
        private void HandleItemsInternalChanged(object sender, EventArgs e)
        {
            ItemsChanged?.Invoke(this, EventArgs.Empty);
        }

        // Event handler for when the control's size changes
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            DrawList(false);
        }
        // Event handler for when the control's sgot an item clicked
        private void HandleItemClicked(object sender, TileListItemEventArgs e)
        {
            TileListItem item = e.Item;
            ItemDoubleClicked?.Invoke(this, new TileListItemEventArgs(item));
        }

        // Internal fields and properties
        private int RowX { get; set; }
        private int RowY { get; set; }
        private int RowCount { get; set; }
        private int RowMaxCount { get; set; }
        private int _height { get; set; }
        private int _width { get; set; }
        private int _picture_width { get; set; }
        private int _picture_height { get; set; }
        private int _Margin { get; set; }

        // Public fields and properties
        [Category("Data")]
        public TileListCollection Items { get; set; }

        [Browsable(true)]
        [Category("Tile")]
        public new int Height { get { return _height; } set { ItemsChanged?.Invoke(this, EventArgs.Empty); _height = value; } }
        [Browsable(true)]
        [Category("Tile")]
        public new int Width { get { return _width; } set{ ItemsChanged?.Invoke(this, EventArgs.Empty); _width = value;} }
        [Browsable(true)]
        [Category("Tile")]
        public int Picture_Height { get { return _picture_height; } set { ItemsSizeChanged?.Invoke(this, EventArgs.Empty); _picture_height = value; } }
        [Browsable(true)]
        [Category("Tile")]
        public int Picture_Width { get { return _picture_width; } set { ItemsSizeChanged?.Invoke(this, EventArgs.Empty); _picture_width = value; } }
        [Browsable(true)]
        [Category("Tile")]
        public new int Margin { get { return _Margin; } set { ItemsSizeChanged?.Invoke(this, EventArgs.Empty); _Margin = value; } }

        // Constructor
        public TileListView()
        {
            RowX = 0;
            RowY = 0;
            RowCount = 0;
            Width = 95;
            Height = 125;

            Items = new TileListCollection();
            Items.InternalItemChanged += HandleItemsInternalChanged;
            Items.ItemChanged += HandleItemChanged;
            Items.ItemDoubleClicked += HandleItemClicked;
            this.ItemsSizeChanged += ResizeItems;
            this.AutoScroll = true;
            this.HorizontalScroll.Enabled = false;
        }

        // Internal functions
        private void ResizeItems(object sender, EventArgs e)
        {
            foreach (TileListItem item in this.Items)
            {
                item.Picture_Width = Picture_Width;
                item.Picture_Height = Picture_Height;
                item.Margin = new System.Windows.Forms.Padding(Margin,Margin,Margin,Margin);
            }
        }

        private void DrawList(bool must_be_draw)
        {
            if ((this.ClientSize.Width / Width) != RowMaxCount || must_be_draw == true)
            {
                RowX = 0;
                RowY = 0;
                RowCount = 0;
                this.Controls.Clear();
                foreach (TileListItem item in this.Items)
                {
                    RowMaxCount = this.ClientSize.Width / Width;

                    if (RowCount < RowMaxCount)
                    {
                        item.Location = new Point(RowX, RowY);
                        item.Size = new Size(Width, Height);
                        this.Controls.Add(item);
                        RowX += Width;
                        RowCount++;
                    }
                    else
                    {
                        RowX = 0;
                        RowY += Height;
                        RowCount = -1;
                        item.Location = new Point(RowX, RowY);
                        item.Size = new Size(Width, Height);
                        this.Controls.Add(item);
                        RowCount++;
                    }
                }
                this.AutoScrollMinSize = new Size(0, RowY + (RowCount > 0 ? Height : 0));
            }
        }
    }
}
