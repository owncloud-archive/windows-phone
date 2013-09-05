using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Phone.Controls;

namespace OwnCloud.View.Controls
{
    class ProgressOverlayPopup
    {
        ProgressOverlayControl _control;
        Popup _popup;

        public ProgressOverlayPopup(string defaultText = "")
        {
            _popup = new Popup();
            _control = new ProgressOverlayControl(defaultText);
            _popup.Child = _control;
        }

        /// <summary>
        /// Returns the user control.
        /// </summary>
        public ProgressOverlayControl Control
        {
            get
            {
                return _control;
            }
        }

        public Color BackgroundColor
        {
            get
            {
                return ((SolidColorBrush)_control.Background).Color;
            }
            set
            {
                ((SolidColorBrush)_control.Background).Color = value;
            }
        }

        /// <summary>
        /// The default Canvas zIndex.
        /// </summary>
        public int ZIndex
        {
            get;
            set;
        }


        /// <summary>
        /// If true the control will not change the desired width or height once is set. Otherwise
        /// it will capture the current height of the executing page.
        /// </summary>
        public bool CustomDimension
        {
            get;
            set;
        }

        /// <summary>
        /// Called after Show() has completed the animation.
        /// </summary>
        public event EventHandler ShowCompleted;
        /// <summary>
        /// Called after Hide() has completed the animation.
        /// </summary>
        public event EventHandler HideCompleted;

        /// <summary>
        /// An action called 
        /// </summary>
        public Action OnAfterShow
        {
            get;
            set;
        }

        /// <summary>
        /// Fade-In the control with 500ms.
        /// </summary>
        public void Show()
        {
            Show(500);
        }

        /// <summary>
        /// Fade-In the control.
        /// </summary>
        /// <param name="fadeIn"></param>
        public void Show(int fadeIn)
        {
            Storyboard board = new Storyboard();
            DoubleAnimation ani = new DoubleAnimation() { From = 0, To = 1, Duration = new TimeSpan(0,0,0,0,fadeIn) };
            board.Children.Add(ani);
            Storyboard.SetTarget(ani, Control);
            Storyboard.SetTargetProperty(ani, new PropertyPath(UIElement.OpacityProperty));
            board.Completed += new EventHandler(OnShowCompleted);

            if (!CustomDimension)
            {
                Size cSz = Application.Current.RootVisual.RenderSize;
                Control.Height = cSz.Height;
                Control.Width = cSz.Width;
            }
            _popup.IsOpen = true;
            Control.Visibility = System.Windows.Visibility.Visible;
            Canvas.SetTop(Control, 0);
            Canvas.SetZIndex(Control, ZIndex);
            Control.ProgressBar.IsIndeterminate = true;
            board.Begin();
        }

        /// <summary>
        /// Fade-Out the control with 500ms.
        /// </summary>
        public void Hide()
        {
            Hide(500);
        }

        /// <summary>
        /// Fade-Out the control.
        /// </summary>
        /// <param name="fadeOut"></param>
        public void Hide(int fadeOut)
        {
            Storyboard board = new Storyboard();
            DoubleAnimation ani = new DoubleAnimation() { From = 1, To = 0, Duration = new TimeSpan(0,0,0,0,fadeOut) };
            board.Children.Add(ani);
            Storyboard.SetTarget(ani, Control);
            Storyboard.SetTargetProperty(ani, new PropertyPath(UIElement.OpacityProperty));

            board.Completed += new EventHandler(delegate
            {
                Control.ProgressBar.IsIndeterminate = false;
                Control.Visibility = System.Windows.Visibility.Collapsed;
                _popup.IsOpen = false;
            });
            board.Completed += new EventHandler(OnHideCompleted);
            board.Begin();
        }

        protected void OnShowCompleted(object obj, EventArgs e)
        {
            if (ShowCompleted != null)
            {
                ShowCompleted(this, e);
            }
        }

        protected void OnHideCompleted(object obj, EventArgs e)
        {
            if (HideCompleted != null)
            {
                HideCompleted(this, e);
            }
        }
    }
}
