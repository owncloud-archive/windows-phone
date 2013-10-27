using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace OwnCloud.Extensions
{
    static class FrameworkElementExtensions
    {
        /// <summary>
        /// Fades out an element.
        /// This will create an Animation on the fly.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="time">Fadeout time in milliseconds.</param>
        /// <param name="OnComplete">An action to execute if the animation is complete.</param>
        static public void FadeOut(this FrameworkElement obj, int time = 500, Action OnComplete = null)
        {
            DoubleAnimation ani = new DoubleAnimation()
            {
                Duration = new TimeSpan(0, 0, 0, 0, time),
                From = obj.Opacity,
                To = 0
            };
            Storyboard board = new Storyboard();
            Storyboard.SetTarget(ani, obj);
            Storyboard.SetTargetProperty(ani, new PropertyPath(UIElement.OpacityProperty));
            board.Children.Add(ani);
            if (OnComplete != null)
            {
                board.Completed += new EventHandler((o, e) =>
                {
                    OnComplete();
                });
            }
            board.Begin();
        }

        /// <summary>
        /// Fades in an element.
        /// This will create an Animation on the fly.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="time">Fadein time in milliseconds.</param>
        /// <param name="OnComplete">An action to execute if the animation is complete.</param>
        static public void FadeIn(this FrameworkElement obj, int time = 500, Action OnComplete = null)
        {
            DoubleAnimation ani = new DoubleAnimation()
            {
                Duration = new TimeSpan(0, 0, 0, 0, time),
                From = obj.Opacity,
                To = 1
            };
            Storyboard board = new Storyboard();
            Storyboard.SetTarget(ani, obj);
            Storyboard.SetTargetProperty(ani, new PropertyPath(UIElement.OpacityProperty));
            board.Children.Add(ani);
            if (OnComplete != null)
            {
                board.Completed += new EventHandler((o, e) =>
                {
                    OnComplete();
                });
            }
            board.Begin();
        }

        /// <summary>
        /// Simple calls an action after a delay.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="time">The delayed time in milliseconds.</param>
        /// <param name="OnComplete">An action to call after delay.</param>
        static public void Delay(this FrameworkElement obj, int time, Action OnComplete)
        {
            DispatcherTimer timer = new DispatcherTimer()
            {
                Interval = new TimeSpan(0,0,0,0,time)
            };
            timer.Tick += new EventHandler((o, e) =>
            {
                timer.Stop();
                if (OnComplete != null)
                {
                    OnComplete();
                }
            });
            timer.Start();
        }

        /// <summary>
        /// Immediately collapsed an element.
        /// </summary>
        /// <param name="obj"></param>
        static public void Hide(this FrameworkElement obj)
        {
            obj.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Immediately show an collapsed element.
        /// </summary>
        /// <param name="obj"></param>
        static public void Show(this FrameworkElement obj)
        {
            obj.Visibility = Visibility.Visible;
        }
    }
}
