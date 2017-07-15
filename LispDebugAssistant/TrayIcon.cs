using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace nucs.WinForms.Tray {
    public class TrayIcon : IDisposable {
        private BalloonTipClickHandlerRegistration _balloonTipClickHandlers;
        private Icon _icon;
        private bool _showingDefaultBalloonTip;

        public TrayIcon(Icon icon) {
            Init(icon);
        }

        public TrayIcon(Stream iconImageStream) {
            InitWithStream(iconImageStream);
        }

        public TrayIcon(Bitmap iconBitmap) {
            Init(Icon.FromHandle(iconBitmap.GetHicon()));
        }

        public TrayIcon(string pathToIcon) {
            using (var fileStream = new FileStream(pathToIcon, FileMode.Open, FileAccess.Read))
                InitWithStream(fileStream);
        }

        public NotifyIcon NotifyIcon { get; private set; }
        public int DefaultBalloonTipTimeout { get; set; }

        public Icon Icon {
            get { return _icon; }
            set {
                _icon = value;
                NotifyIcon.Icon = _icon;
            }
        }

        public string DefaultTipText { get; set; }
        public string DefaultTipTitle { get; set; }
        public Action DefaultBalloonTipClickedAction { get; set; }
        public Action DefaultBalloonTipClosedAction { get; set; }

        public void Dispose() {
            lock (this) {
                NotifyIcon?.Dispose();
                NotifyIcon = null;
            }
        }

        private void InitWithStream(Stream iconImageStream) {
            var icon = new Icon(iconImageStream);
            Init(icon);
        }

        private void Init(Icon icon) {
            _icon = icon;
            DefaultBalloonTipTimeout = 2000;
            NotifyIcon = new NotifyIcon {ContextMenu = new ContextMenu()};
            NotifyIcon.MouseMove += ShowDefaultBalloonTip;
            NotifyIcon.BalloonTipClicked += BalloonTipClickedHandler;
            NotifyIcon.BalloonTipClosed += BalloonTipClosedHandler;
        }

        private void BalloonTipClosedHandler(object sender, EventArgs e) {
            var toRun = GetCustomBalloonTipClosedAction() ?? DefaultBalloonTipClosedAction;
            toRun?.Invoke();
        }

        private void BalloonTipClickedHandler(object sender, EventArgs e) {
            var toRun = GetCustomBalloonTipClickAction() ?? DefaultBalloonTipClickedAction;
            toRun?.Invoke();
        }

        private void ShowDefaultBalloonTip(object sender, MouseEventArgs e) {
            if (string.IsNullOrEmpty(DefaultTipText) || string.IsNullOrEmpty(DefaultTipTitle))
                return;
            lock (this) {
                if (HaveRegisteredClickHandlers())
                    return;
                if (_showingDefaultBalloonTip)
                    return;
                _showingDefaultBalloonTip = true;
            }
            ShowBalloonTipFor(DefaultBalloonTipTimeout, DefaultTipTitle, DefaultTipText, ToolTipIcon.Info, DefaultBalloonTipClickedAction,
                () => {
                    _showingDefaultBalloonTip = false;
                    _balloonTipClickHandlers = null;
                    var closedAction = DefaultBalloonTipClosedAction;
                    closedAction?.Invoke();
                });
        }

        private bool HaveRegisteredClickHandlers() {
            lock (this) {
                return CustomBalloonTipHandlerExists_UNLOCKED();
            }
        }

        private bool CustomBalloonTipHandlerExists_UNLOCKED() {
            return (_balloonTipClickHandlers != null);
        }

        private Action GetCustomBalloonTipClickAction() {
            lock (this) {
                if (!CustomBalloonTipHandlerExists_UNLOCKED())
                    return null;
                return _balloonTipClickHandlers.ClickAction;
            }
        }

        private Action GetCustomBalloonTipClosedAction() {
            lock (this) {
                if (!CustomBalloonTipHandlerExists_UNLOCKED())
                    return null;
                return _balloonTipClickHandlers.ClosedAction;
            }
        }

        public void ShowBalloonTipFor(int timeoutInMilliseconds, string title, string text, ToolTipIcon icon,
            Action clickAction = null, Action closeAction = null) {
            lock (this) {
                _balloonTipClickHandlers = new BalloonTipClickHandlerRegistration(clickAction, closeAction);
            }
            NotifyIcon.ShowBalloonTip(timeoutInMilliseconds, title, text, icon);
        }

        public Menu.MenuItemCollection MenuItems => NotifyIcon?.ContextMenu?.MenuItems;

        public void AddMenuItem(string withText, Action withCallback) {
            lock (this) {
                if (NotifyIcon == null)
                    return;
                var menuItem = new MenuItem {
                    Text = withText
                };
                if (withCallback != null)
                    menuItem.Click += (s, e) => withCallback();
                NotifyIcon.ContextMenu.MenuItems.Add(menuItem);
            }
        }

        public void AddMenuSeparator() {
            AddMenuItem("-", null);
        }

        public void RemoveMenuItem(string withText) {
            lock (this) {
                if (NotifyIcon == null)
                    return;
                foreach (MenuItem mi in NotifyIcon.ContextMenu.MenuItems)
                    if (mi.Text == withText) {
                        NotifyIcon.ContextMenu.MenuItems.Remove(mi);
                        return;
                    }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nameGetter">Before menu opens, the name will be fetched from this func.</param>
        /// <param name="onclick">On click, this is passed along with the name.</param>
        public void AddChangingItemMenu(Func<MenuItem, string> nameGetter, Action<MenuItem, string> onclick) {
            lock (this) {
                if (NotifyIcon == null)
                    return;
                var menuItem = new MenuItem { };
                var n = nameGetter(menuItem);
                menuItem.Text = n;
                if (onclick != null)
                    menuItem.Click += (s, e) => { onclick(menuItem, menuItem.Text); };
                NotifyIcon.ContextMenu.MenuItems.Add(menuItem);
                NotifyIcon.ContextMenu.Popup += (sender, args) => {
                    if (MenuItems?.Contains(menuItem) == true)
                        menuItem.Text = nameGetter(menuItem);
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">The name of the bool</param>
        /// <param name="getState">Gets the value of the bool</param>
        /// <param name="setState">Sets the value of the bool</param>
        public void AddBooleanMenuItem(string name, Func<bool> getState, Action<bool> setState) {
            AddChangingItemMenu(item => $"{name}: {(getState() ? "Enabled" : "Disabled")}", (item, s) => setState(!getState()));
        }

        public void AddShowHideMenuItem(Form form, string show = "Show", string hide = "Hide") {
            AddChangingItemMenu(item => (item?.Text ?? "") == "" ? hide : (form.Visible ? hide : show), (item, s) => { form.Invoke(new MethodInvoker(() => { form.Visible = s == show; })); });
        }

        public void AddItemsGenerator(Func<IEnumerable<string>> generate, Func<string> @default, Action<MenuItem> onclick) {
            string u = new Random().Next(0, 10000000).ToString();
            NotifyIcon.ContextMenu.MenuItems.Add(new MenuItem(@default()) {Tag = $"D{u}"});

            var del = new Action(() => {
                foreach (var mi in NotifyIcon.ContextMenu.MenuItems.Cast<MenuItem>().ToArray()) {
                    if (mi.Tag as string == u)
                        NotifyIcon.ContextMenu.MenuItems.Remove(mi);
                }
            });
            var getdefault = new Func<MenuItem>(() => NotifyIcon.ContextMenu.MenuItems.Cast<MenuItem>().First(item => (string) item.Tag == $"D{u}"));
            var setdefault = new Action(() => getdefault().Text = @default());
            var setdefault_vis = new Action<bool>(@do => getdefault().Visible = @do);


            NotifyIcon.ContextMenu.Popup += (sender, args) => {
                var i = generate()?.ToArray() ?? new string[0];
                del();
                if (i.Length == 0) {
                    setdefault();
                    setdefault_vis(true);
                } else {
                    var index = NotifyIcon.ContextMenu.MenuItems.IndexOf(getdefault());
                    var items = NotifyIcon.ContextMenu.MenuItems.Cast<MenuItem>().ToList();
                    var l = new List<MenuItem>();
                    NotifyIcon.ContextMenu.MenuItems.Clear();
                    for (int j = 0; j < index; j++) {
                        l.Add(items[j]);
                    }
                    l.Add(new MenuItem(@default()) {Tag = $"D{u}", Visible = false});

                    foreach (var item in i) {
                        l.Add(new MenuItem(item) {Tag = u});
                    }
                    l.ForEach(it => it.Click += (o, argsy) => onclick?.Invoke(it));

                    for (int j = index + 1; j < items.Count; j++) {
                        l.Add(items[j]);
                    }
                    NotifyIcon.ContextMenu.MenuItems.AddRange(l.ToArray());
                }
            };
        }

        public void Show() {
            NotifyIcon.MouseClick += onIconMouseClick;
            NotifyIcon.Icon = _icon;
            NotifyIcon.Visible = true;
        }

        public void Hide() {
            NotifyIcon.Visible = false;
        }

        private void onIconMouseClick(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) { }
        }

        private class BalloonTipClickHandlerRegistration {
            public BalloonTipClickHandlerRegistration(Action clickAction = null, Action closedAction = null) {
                ClickAction = clickAction;
                ClosedAction = closedAction;
            }

            public Action ClickAction { get; }
            public Action ClosedAction { get; }
        }
    }
}