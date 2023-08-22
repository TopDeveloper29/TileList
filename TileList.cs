using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static System.Windows.Forms.ListView;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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
        private void TileListItem_MouseEnter(object sender, EventArgs e)
        {
            if (this.Parent != null)
            {
                Color parentColor = this.Parent.BackColor;
                float fadeAmount = (float)1.3;
                this.BackColor = ControlPaint.Light(parentColor, fadeAmount);
                this.Cursor = Cursors.Hand;
            }

        }

        // Event handler for when the mouse leaves an item
        private void TileListItem_MouseLeave(object sender, EventArgs e)
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

        // Private fields
        private Label label { get; set; }
        private PictureBox pictureBox { get; set; }
        private string _text;
        private Image _image;

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
            picpan.DoubleClick += _DoubleClick;
            labpan.DoubleClick += _DoubleClick;
            this.DoubleClick += _DoubleClick;
        }

        private void _DoubleClick(object sender, EventArgs e)
        {
            OnItemDoubleClicked?.Invoke(this, new TileListItemEventArgs(this));
        }
    }

    // Custom control for displaying a list of file/folder items
    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(TileListListView))]

    // Custom collection class for managing file/folder items
    public class TileListCollection : IEnumerable<TileListItem>
    {
        // Event for when an item in the collection changes
        public event EventHandler ItemChanged;
        public event EventHandler InternalItemChanged;

        // Event for when an item in the collection is double-clicked
        public event EventHandler<TileListItemEventArgs> ItemDoubleClicked;

        // Event handler for double-clicking on an item in the collection
        private void Item_DoubleClicked(object sender, TileListItemEventArgs e)
        {
            TileListItem item = e.Item;
            ItemDoubleClicked?.Invoke(this, e);
        }
        // Event handler for item change on an item in the collection
        private void Item_ChangedFromItem(object sender, EventArgs e)
        {
            InternalItemChanged?.Invoke(this, EventArgs.Empty);
        }

        // Internal fields and properties
        private List<TileListItem> _items = new List<TileListItem>();

        // Constructor
        public TileListCollection() { }

        // Public methods
        public void Add(TileListItem item)
        {
            item.OnItemDoubleClicked += Item_DoubleClicked;
            item.ItemsChanges += Item_ChangedFromItem;
            this._items.Add(item);
            ItemChanged?.Invoke(this, EventArgs.Empty);

        }

        public void Remove(int index)
        {
            _items.RemoveAt(index);
            ItemChanged?.Invoke(this, EventArgs.Empty);

        }

        public void Clear()
        {
            _items.Clear();
            ItemChanged?.Invoke(this, EventArgs.Empty);

        }

        // Methods required for IEnumerable implementation to allow the class to be enumerated properly
        public IEnumerator<TileListItem> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class TileListListView : Panel
    {
        // Event for when an item in the list is double-clicked
        public event EventHandler<TileListItemEventArgs> ItemDoubleClicked;
        public event EventHandler ItemsChanged;

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
        private int RowWidth { get; set; }
        private int RowHeight { get; set; }
        private int RowCount { get; set; }
        private int RowMaxCount { get; set; }

        // Public fields and properties
        public TileListCollection Items { get; }

        // Constructor
        public TileListListView()
        {
            RowX = 0;
            RowY = 0;
            RowCount = 0;
            RowWidth = 95;
            RowHeight = 125;

            Items = new TileListCollection();
            Items.InternalItemChanged += HandleItemsInternalChanged;
            Items.ItemChanged += HandleItemChanged;
            Items.ItemDoubleClicked += HandleItemClicked;

            this.AutoScroll = true;
            this.HorizontalScroll.Enabled = false;
        }

        // Internal functions
        private void DrawList(bool must_be_draw)
        {
            if ((this.ClientSize.Width / RowWidth) != RowMaxCount || must_be_draw == true)
            {
                RowX = 0;
                RowY = 0;
                RowCount = 0;
                this.Controls.Clear();
                foreach (TileListItem item in this.Items)
                {
                    RowMaxCount = this.ClientSize.Width / RowWidth;

                    if (RowCount < RowMaxCount)
                    {
                        item.Location = new Point(RowX, RowY);
                        item.Size = new Size(RowWidth, RowHeight);
                        this.Controls.Add(item);
                        RowX += RowWidth;
                        RowCount++;
                    }
                    else
                    {
                        RowX = 0;
                        RowY += RowHeight;
                        RowCount = -1;
                        item.Location = new Point(RowX, RowY);
                        item.Size = new Size(RowWidth, RowHeight);
                        this.Controls.Add(item);
                        RowCount++;
                    }
                }
                this.AutoScrollMinSize = new Size(0, RowY + (RowCount > 0 ? RowHeight : 0));
            }
        }
    }
}
